#!/usr/bin/env python3

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import signal
import stat
import subprocess
import sys
import tempfile
import time


PREFIX = "[AOTDIAG]"
EXPECTED_RUNTIME_PACK_VERSION = "10.0.2"
SAFE_SWITCHES = {
    "--enable-bulk-memory",
    "--enable-exception-handling",
    "--enable-multivalue",
    "--enable-mutable-globals",
    "--enable-reference-types",
    "--enable-sign-ext",
    "--enable-simd",
    "--enable-tail-call",
    "--enable-threads",
    "--low-memory-unused",
    "--mvp-features",
    "--pass-arg=directize-initial-contents-immutable",
    "--post-emscripten",
    "--strip-debug",
    "--strip-target-features",
    "--zero-filled-memory",
    "-fwasm-exceptions",
    "-pthread",
}
SAFE_SETTINGS = {
    "ALLOW_MEMORY_GROWTH",
    "ASSERTIONS",
    "DISABLE_EXCEPTION_CATCHING",
    "ENVIRONMENT",
    "INITIAL_MEMORY",
    "MAXIMUM_MEMORY",
    "STACK_SIZE",
    "WASM_BIGINT",
}
LIST_SETTINGS = {
    "DEFAULT_LIBRARY_FUNCS_TO_INCLUDE",
    "EXPORTED_FUNCTIONS",
    "EXPORTED_RUNTIME_METHODS",
}
PATH_SWITCHES = {
    "--extern-post-js",
    "--extern-pre-js",
    "--js-library",
    "--post-js",
    "--pre-js",
    "-o",
}
STANDARD_SECTION_NAMES = {
    0: "custom",
    1: "type",
    2: "import",
    3: "function",
    4: "table",
    5: "memory",
    6: "global",
    7: "export",
    8: "start",
    9: "element",
    10: "code",
    11: "data",
    12: "data-count",
    13: "tag",
}
SAFE_CUSTOM_SECTION_NAMES = {
    ".debug_abbrev",
    ".debug_info",
    ".debug_line",
    ".debug_loc",
    ".debug_ranges",
    ".debug_str",
    "dylink.0",
    "external_debug_info",
    "linking",
    "name",
    "producers",
    "reloc.CODE",
    "reloc.DATA",
    "sourceMappingURL",
    "target_features",
}


def append_jsonl(path_value, payload):
    if not path_value:
        return
    path = Path(path_value)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(payload, sort_keys=True, separators=(",", ":")))
        stream.write("\n")


def emit(event, output=None, **fields):
    if output is None:
        output = os.environ.get("UNO_AOT_DIAGNOSTICS_EVENTS")
    payload = {
        "event": event,
        "timestampUtc": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
        **fields,
    }
    rendered = json.dumps(payload, sort_keys=True, separators=(",", ":"))
    try:
        if os.environ.get("UNO_AOT_DIAGNOSTICS_QUIET") != "1":
            print(f"{PREFIX} {rendered}", flush=True)
    except (BrokenPipeError, OSError):
        pass
    try:
        append_jsonl(output, payload)
    except OSError as error:
        warning = {
            "event": "diagnostic-warning",
            "timestampUtc": payload["timestampUtc"],
            "scope": "event-artifact",
            "status": "unavailable",
            "errorType": type(error).__name__,
        }
        try:
            print(
                f"{PREFIX} "
                f"{json.dumps(warning, sort_keys=True, separators=(',', ':'))}",
                flush=True,
            )
        except (BrokenPipeError, OSError):
            pass


def digest_text(value):
    return hashlib.sha256(value.encode("utf-8", errors="replace")).hexdigest()


def safe_basename(value):
    normalized = value.strip().strip("\"'")
    return Path(normalized.replace("\\", "/")).name or "<none>"


def safe_option_name(value):
    match = re.match(r"^(--[A-Za-z0-9][A-Za-z0-9-]*|-[A-Za-z])", value)
    return match.group(1) if match else "<redacted-option>"


def split_setting(line):
    match = re.fullmatch(r"-s\s+([A-Z0-9_]+)=(.*)", line)
    if not match:
        return None
    return match.group(1), match.group(2)


def sanitize_response_line(index, raw_line):
    line = raw_line.strip()
    if not line:
        return {"index": index, "category": "empty", "value": "<empty>"}

    if re.fullmatch(r"-g(?:[0-3])?", line):
        return {"index": index, "category": "debug", "value": line}
    if re.fullmatch(r"-O(?:[0-3]|g|s|z)", line):
        return {"index": index, "category": "optimization", "value": line}

    setting = split_setting(line)
    if setting:
        name, value = setting
        if name in SAFE_SETTINGS and re.fullmatch(r"[A-Za-z0-9_.,:+-]+", value):
            return {
                "index": index,
                "category": "setting",
                "name": name,
                "value": value,
            }
        if name in LIST_SETTINGS:
            return {
                "index": index,
                "category": "setting-list",
                "name": name,
                "characterCount": len(value),
                "sha256": digest_text(value),
            }
        return {
            "index": index,
            "category": "setting-redacted",
            "name": "<redacted-setting>",
            "nameSha256": digest_text(name),
            "characterCount": len(value),
            "sha256": digest_text(value),
        }

    try:
        tokens = shlex.split(line, posix=True)
    except ValueError:
        tokens = []

    if tokens:
        first = tokens[0]
        if first in PATH_SWITCHES:
            return {
                "index": index,
                "category": "path-option",
                "option": first,
                "value": safe_basename(tokens[1]) if len(tokens) > 1 else "<missing>",
            }
        if len(tokens) == 1 and first in SAFE_SWITCHES:
            return {"index": index, "category": "flag", "value": first}

    if line.startswith("-"):
        return {
            "index": index,
            "category": "option-redacted",
            "option": safe_option_name(line),
            "characterCount": len(line),
            "sha256": digest_text(line),
        }

    return {
        "index": index,
        "category": "input",
        "value": safe_basename(line),
        "sha256": digest_text(line),
    }


def response_command(args):
    response_path = Path(args.path)
    raw = response_path.read_bytes()
    lines = raw.decode("utf-8-sig", errors="replace").splitlines()
    items = [sanitize_response_line(index, line) for index, line in enumerate(lines)]
    debug_flags = [
        {"index": item["index"], "value": item["value"]}
        for item in items
        if item["category"] == "debug"
    ]
    optimization_flags = [
        {"index": item["index"], "value": item["value"]}
        for item in items
        if item["category"] == "optimization"
    ]
    document = {
        "schemaVersion": 1,
        "responseFile": response_path.name,
        "responseSha256": hashlib.sha256(raw).hexdigest(),
        "itemCount": len(items),
        "debugFlagsInOrder": debug_flags,
        "optimizationFlagsInOrder": optimization_flags,
        "items": items,
    }
    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(document, indent=2) + "\n", encoding="utf-8")
    emit(
        "link-response-summary",
        output=args.events,
        responseFile=response_path.name,
        responseSha256=document["responseSha256"],
        itemCount=len(items),
        debugFlagsInOrder=debug_flags,
        optimizationFlagsInOrder=optimization_flags,
    )
    for item in items:
        emit("link-response-item", output=args.events, **item)
    return 0


def read_u32_leb(stream):
    value = 0
    shift = 0
    for _ in range(5):
        byte = stream.read(1)
        if not byte:
            raise ValueError("Unexpected end of file while reading LEB128.")
        number = byte[0]
        value |= (number & 0x7F) << shift
        if number & 0x80 == 0:
            return value
        shift += 7
    raise ValueError("Invalid u32 LEB128 value.")


def inspect_wasm(path):
    path = Path(path)
    total_size = path.stat().st_size
    sections = []
    with path.open("rb") as stream:
        if stream.read(8) != b"\x00asm\x01\x00\x00\x00":
            raise ValueError("Input is not a WebAssembly 1.0 module.")
        index = 0
        while stream.tell() < total_size:
            section_id_raw = stream.read(1)
            if not section_id_raw:
                break
            section_id = section_id_raw[0]
            payload_size = read_u32_leb(stream)
            payload_start = stream.tell()
            entry = {
                "index": index,
                "id": section_id,
                "name": STANDARD_SECTION_NAMES.get(section_id, "unknown"),
                "payloadBytes": payload_size,
            }
            if section_id == 0 and payload_size:
                name_length = read_u32_leb(stream)
                name_bytes = stream.read(min(name_length, 256))
                if name_length > len(name_bytes):
                    stream.seek(name_length - len(name_bytes), os.SEEK_CUR)
                custom_name = name_bytes.decode("utf-8", errors="replace")
                entry["customName"] = (
                    custom_name
                    if custom_name in SAFE_CUSTOM_SECTION_NAMES
                    else "<redacted>"
                )
                entry["customNameSha256"] = digest_text(custom_name)
            stream.seek(payload_start + payload_size)
            sections.append(entry)
            index += 1
    return {"byteSize": total_size, "sections": sections}


def sanitize_optimizer_arguments(arguments):
    safe = []
    skip_path = False
    for index, value in enumerate(arguments):
        if skip_path:
            skip_path = False
            continue
        if value in PATH_SWITCHES:
            following = arguments[index + 1] if index + 1 < len(arguments) else ""
            safe.append(
                {
                    "index": index,
                    "category": "path-option",
                    "option": value,
                    "value": safe_basename(following),
                }
            )
            skip_path = True
        elif re.fullmatch(r"-g(?:[0-3])?", value):
            safe.append({"index": index, "category": "debug", "value": value})
        elif re.fullmatch(r"-O(?:[0-3]|g|s|z)", value):
            safe.append({"index": index, "category": "optimization", "value": value})
        elif value in SAFE_SWITCHES:
            safe.append({"index": index, "category": "flag", "value": value})
        elif value.endswith(".wasm"):
            safe.append(
                {
                    "index": index,
                    "category": "wasm-input",
                    "value": safe_basename(value),
                }
            )
        elif value.startswith("-"):
            safe.append(
                {
                    "index": index,
                    "category": "option-redacted",
                    "option": safe_option_name(value),
                    "characterCount": len(value),
                    "sha256": digest_text(value),
                }
            )
        else:
            safe.append(
                {
                    "index": index,
                    "category": "value-redacted",
                    "characterCount": len(value),
                    "sha256": digest_text(value),
                }
            )
    return safe


def wasm_opt_command(args):
    optimizer_args = args.arguments
    if optimizer_args and optimizer_args[0] == "--":
        optimizer_args = optimizer_args[1:]
    output = os.environ.get("UNO_AOT_DIAGNOSTICS_EVENTS")
    is_post_link = "--post-emscripten" in optimizer_args
    if is_post_link:
        try:
            safe_arguments = sanitize_optimizer_arguments(optimizer_args)
            emit(
                "wasm-opt-invocation",
                output=output,
                arguments=safe_arguments,
                debugFlagsInOrder=[
                    item
                    for item in safe_arguments
                    if item.get("category") == "debug"
                ],
                optimizationFlagsInOrder=[
                    item
                    for item in safe_arguments
                    if item.get("category") == "optimization"
                ],
            )
            input_path = next(
                (
                    Path(value)
                    for value in optimizer_args
                    if value.endswith(".wasm") and Path(value).is_file()
                ),
                None,
            )
            if input_path is not None:
                metadata = inspect_wasm(input_path)
                emit(
                    "wasm-opt-input",
                    output=output,
                    inputFile=input_path.name,
                    **metadata,
                )
            else:
                emit(
                    "diagnostic-warning",
                    output=output,
                    scope="wasm-opt-input",
                    status="unavailable",
                    reason="input-file-not-found",
                )
        except Exception as error:
            emit(
                "diagnostic-warning",
                output=output,
                scope="wasm-opt-metadata",
                status="unavailable",
                errorType=type(error).__name__,
            )

    child = None

    def forward_signal(signal_number, _frame):
        if child is not None and child.poll() is None:
            child.send_signal(signal_number)

    signal.signal(signal.SIGINT, forward_signal)
    signal.signal(signal.SIGTERM, forward_signal)
    child = subprocess.Popen([args.real, *optimizer_args])
    return_code = child.wait()
    if is_post_link:
        if return_code < 0:
            emit(
                "wasm-opt-exit",
                output=output,
                returnCode=return_code,
                signal=-return_code,
            )
        else:
            emit("wasm-opt-exit", output=output, returnCode=return_code)
    return 128 + (-return_code) if return_code < 0 else return_code


def optional_version(command):
    try:
        completed = subprocess.run(
            command,
            check=False,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            timeout=30,
        )
    except (OSError, subprocess.SubprocessError) as error:
        return {
            "status": "unavailable",
            "errorType": type(error).__name__,
        }
    if completed.returncode != 0:
        return {
            "status": "unavailable",
            "returnCode": completed.returncode,
        }
    lines = completed.stdout.splitlines()
    return {
        "status": "available",
        "value": lines[0].strip() if lines else "<empty>",
    }


def find_toolchain(dotnet_root, runtime_pack_version):
    pack_root = dotnet_root / "packs"
    candidates = sorted(
        pack_root.glob(
            "Microsoft.NET.Runtime.Emscripten.*.Sdk.linux-x64/"
            f"{runtime_pack_version}/tools/bin/wasm-opt"
        )
    )
    if len(candidates) != 1:
        raise RuntimeError(
            f"Expected one Emscripten wasm-opt for {runtime_pack_version}, "
            f"found {len(candidates)}."
        )
    wasm_opt = candidates[0]
    emcc = wasm_opt.parent.parent / "emscripten" / "emcc"
    if not emcc.is_file():
        raise RuntimeError("The matching Emscripten emcc executable was not found.")
    return wasm_opt, emcc


def safe_pack_versions(dotnet_root):
    result = {}
    pack_root = dotnet_root / "packs"
    for pattern in (
        "Microsoft.NET.Runtime.WebAssembly.Sdk",
        "Microsoft.NETCore.App.Runtime.Mono.browser-wasm",
        "Microsoft.NET.Runtime.Emscripten.*.Sdk.linux-x64",
        "Microsoft.NET.Runtime.Emscripten.*.Node.linux-x64",
    ):
        for pack in sorted(pack_root.glob(pattern)):
            if not pack.is_dir():
                continue
            try:
                result[pack.name] = sorted(
                    child.name for child in pack.iterdir() if child.is_dir()
                )
            except OSError:
                result[pack.name] = ["unavailable"]
    return result


def safe_directory_versions(path):
    try:
        return sorted(child.name for child in path.iterdir() if child.is_dir())
    except OSError:
        return ["unavailable"]


def install_wrapper_command(args, version_probe=optional_version):
    dotnet_root = Path(args.dotnet_root).resolve()
    wasm_opt, emcc = find_toolchain(dotnet_root, args.runtime_pack_version)
    backup = wasm_opt.with_name(f"{wasm_opt.name}.uno-aot-diagnostics-real")
    if backup.exists():
        raise RuntimeError("A diagnostics wasm-opt backup already exists.")
    pack_name = wasm_opt.parents[3].name
    emscripten_match = re.fullmatch(
        r"Microsoft\.NET\.Runtime\.Emscripten\.(.+)\.Sdk\.linux-x64",
        pack_name,
    )
    wasm_opt_version = version_probe([str(wasm_opt), "--version"])
    if wasm_opt_version["status"] != "available":
        emit(
            "diagnostic-warning",
            scope="wasm-opt-version",
            status="unavailable",
            **{
                key: value
                for key, value in wasm_opt_version.items()
                if key != "status"
            },
        )
    emit(
        "toolchain",
        dotnetSdkVersions=safe_directory_versions(dotnet_root / "sdk"),
        emscriptenVersion=(
            emscripten_match.group(1) if emscripten_match else "unavailable"
        ),
        emscriptenSdkPack=pack_name,
        emccMetadata="static-pack-metadata",
        wasmOptVersion=wasm_opt_version,
        packs=safe_pack_versions(dotnet_root),
        expectedRuntimePackVersion=args.runtime_pack_version,
    )
    original_mode = stat.S_IMODE(wasm_opt.stat().st_mode)
    wrapper = "\n".join(
        (
            "#!/usr/bin/env python3",
            "import os",
            "import sys",
            f"SCRIPT = {str(Path(args.script).resolve())!r}",
            f"REAL = {str(backup)!r}",
            "os.execv(sys.executable, [sys.executable, SCRIPT, 'wasm-opt', "
            "'--real', REAL, '--', *sys.argv[1:]])",
            "",
        )
    )
    temporary_wrapper = wasm_opt.with_name(f".{wasm_opt.name}.uno-aot-diagnostics")
    temporary_wrapper.write_text(wrapper, encoding="utf-8", newline="\n")
    temporary_wrapper.chmod(original_mode)
    os.replace(wasm_opt, backup)
    try:
        os.replace(temporary_wrapper, wasm_opt)
    except Exception:
        os.replace(backup, wasm_opt)
        temporary_wrapper.unlink(missing_ok=True)
        raise
    emit(
        "wasm-opt-wrapper-installed",
        binaryName=wasm_opt.name,
        runtimePackVersion=args.runtime_pack_version,
    )
    return 0


def restore_wrapper_command(args):
    dotnet_root = Path(args.dotnet_root).resolve()
    wasm_opt, _ = find_toolchain(dotnet_root, args.runtime_pack_version)
    backup = wasm_opt.with_name(f"{wasm_opt.name}.uno-aot-diagnostics-real")
    if backup.exists():
        wasm_opt.unlink(missing_ok=True)
        os.replace(backup, wasm_opt)
        emit(
            "wasm-opt-wrapper-restored",
            binaryName=wasm_opt.name,
            runtimePackVersion=args.runtime_pack_version,
        )
    return 0


def normalized_return_code(return_code):
    return 128 + (-return_code) if return_code < 0 else return_code


def run_publisher_core(command, install_wrapper, restore_wrapper, start_monitor):
    publisher = None
    monitor = None
    wrapper_installed = False
    previous_handlers = {}
    pending_signal = None

    def forward_signal(signal_number, _frame):
        nonlocal pending_signal
        if publisher is not None and publisher.poll() is None:
            publisher.send_signal(signal_number)
        elif pending_signal is None:
            pending_signal = signal_number

    for signal_number in (signal.SIGINT, signal.SIGTERM):
        previous_handlers[signal_number] = signal.getsignal(signal_number)
        signal.signal(signal_number, forward_signal)

    try:
        try:
            wrapper_installed = install_wrapper()
        except Exception as error:
            emit(
                "diagnostic-warning",
                scope="wrapper-install",
                status="unavailable",
                errorType=type(error).__name__,
            )

        if pending_signal is not None:
            return_code = 128 + pending_signal
            emit(
                "publisher-cancelled-before-start",
                returnCode=return_code,
                signal=pending_signal,
            )
            return return_code

        publisher = subprocess.Popen(command)
        if pending_signal is not None and publisher.poll() is None:
            publisher.send_signal(pending_signal)
            pending_signal = None
        try:
            monitor = start_monitor(publisher.pid)
        except Exception as error:
            emit(
                "diagnostic-warning",
                scope="compiler-telemetry",
                status="unavailable",
                errorType=type(error).__name__,
            )

        return_code = publisher.wait()
        if return_code < 0:
            emit(
                "publisher-exit",
                returnCode=return_code,
                signal=-return_code,
            )
        else:
            emit("publisher-exit", returnCode=return_code)
        return normalized_return_code(return_code)
    finally:
        if monitor is not None and monitor.poll() is None:
            monitor.terminate()
            try:
                monitor.wait(timeout=10)
            except subprocess.TimeoutExpired:
                monitor.kill()
                monitor.wait()
        if wrapper_installed:
            try:
                restore_wrapper()
            except Exception as error:
                emit(
                    "diagnostic-warning",
                    scope="wrapper-restore",
                    status="unavailable",
                    errorType=type(error).__name__,
                )
        for signal_number, handler in previous_handlers.items():
            signal.signal(signal_number, handler)


def run_publisher_command(args):
    command = args.publisher
    if command and command[0] == "--":
        command = command[1:]
    if not command:
        raise ValueError("A publisher command is required.")

    os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = args.events
    install_args = argparse.Namespace(
        dotnet_root=args.dotnet_root,
        runtime_pack_version=args.runtime_pack_version,
        script=args.script,
    )
    restore_args = argparse.Namespace(
        dotnet_root=args.dotnet_root,
        runtime_pack_version=args.runtime_pack_version,
    )

    def install():
        return install_wrapper_command(install_args) == 0

    def restore():
        restore_wrapper_command(restore_args)

    def start_monitor(root_pid):
        if args.disable_monitor:
            return None
        return subprocess.Popen(
            [
                sys.executable,
                args.script,
                "monitor",
                "--root-pid",
                str(root_pid),
                "--output",
                args.events,
                "--interval-seconds",
                str(args.interval_seconds),
            ]
        )

    return run_publisher_core(command, install, restore, start_monitor)


def read_processes():
    completed = subprocess.run(
        ["ps", "-eo", "pid=,ppid=,comm=,rss=,pcpu="],
        check=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
    )
    processes = {}
    for line in completed.stdout.splitlines():
        fields = line.split(None, 4)
        if len(fields) != 5:
            continue
        try:
            pid = int(fields[0])
            ppid = int(fields[1])
            rss_kb = int(fields[3])
            cpu_percent = float(fields[4])
        except ValueError:
            continue
        processes[pid] = {
            "pid": pid,
            "ppid": ppid,
            "name": fields[2][:80],
            "rssKb": rss_kb,
            "cpuPercent": cpu_percent,
        }
    return processes


def descendant_processes(processes, root_pid):
    descendants = {root_pid}
    changed = True
    while changed:
        changed = False
        for process in processes.values():
            if process["ppid"] in descendants and process["pid"] not in descendants:
                descendants.add(process["pid"])
                changed = True
    return [processes[pid] for pid in descendants if pid in processes]


def read_integer(path):
    try:
        value = path.read_text(encoding="ascii").strip()
    except OSError:
        return None
    if value == "max":
        return value
    try:
        return int(value)
    except ValueError:
        return None


def cgroup_directory():
    try:
        lines = Path("/proc/self/cgroup").read_text(encoding="ascii").splitlines()
    except OSError:
        return None
    for line in lines:
        fields = line.split(":", 2)
        if len(fields) == 3 and fields[0] == "0":
            candidate = Path("/sys/fs/cgroup") / fields[2].lstrip("/")
            if candidate.is_dir():
                return candidate
    return None


def memory_snapshot():
    result = {}
    try:
        meminfo = {}
        for line in Path("/proc/meminfo").read_text(encoding="ascii").splitlines():
            key, value = line.split(":", 1)
            meminfo[key] = int(value.strip().split()[0])
        result["hostMemTotalKb"] = meminfo.get("MemTotal")
        result["hostMemAvailableKb"] = meminfo.get("MemAvailable")
    except (OSError, ValueError):
        pass

    group = cgroup_directory()
    if group is not None:
        result["cgroupMemoryCurrentBytes"] = read_integer(group / "memory.current")
        result["cgroupMemoryPeakBytes"] = read_integer(group / "memory.peak")
        result["cgroupMemoryMaxBytes"] = read_integer(group / "memory.max")
        try:
            events = {}
            for line in (group / "memory.events").read_text(encoding="ascii").splitlines():
                name, value = line.split()
                if name in {"high", "max", "oom", "oom_group_kill", "oom_kill"}:
                    events[name] = int(value)
            result["cgroupMemoryEvents"] = events
        except (OSError, ValueError):
            pass
    return result


def monitor_command(args):
    missing_samples = 0
    while True:
        processes = read_processes()
        descendants = descendant_processes(processes, args.root_pid)
        if args.root_pid not in processes:
            missing_samples += 1
        else:
            missing_samples = 0
        descendants.sort(key=lambda process: process["rssKb"], reverse=True)
        emit(
            "compiler-telemetry",
            output=args.output,
            rootPid=args.root_pid,
            descendantCount=len(descendants),
            aggregateRssKb=sum(process["rssKb"] for process in descendants),
            processes=descendants[: args.max_processes],
            **memory_snapshot(),
        )
        if missing_samples >= 2:
            emit("compiler-telemetry-complete", output=args.output, rootPid=args.root_pid)
            return 0
        time.sleep(args.interval_seconds)


def encode_u32_leb(value):
    encoded = bytearray()
    while True:
        byte = value & 0x7F
        value >>= 7
        if value:
            byte |= 0x80
        encoded.append(byte)
        if not value:
            return bytes(encoded)


def self_test_command(_args):
    with tempfile.TemporaryDirectory(prefix="uno-gallery-aot-diagnostics-") as temp:
        root = Path(temp)
        fixture_secret = "fixture-secret-shaped-value"
        response = root / "emcc-link.rsp"
        response.write_text(
            "\n".join(
                (
                    "-O2",
                    "-g",
                    "-s INITIAL_MEMORY=33554432",
                    "\"/safe/public/path/Uno.UI.dll.o\"",
                    "-g0",
                    f"--unknown-token={fixture_secret}",
                    f"--pass-arg={fixture_secret}",
                    f"-O{fixture_secret}",
                )
            )
            + "\n",
            encoding="utf-8",
        )
        output = root / "response.json"
        response_command(
            argparse.Namespace(
                path=str(response),
                output=str(output),
                events=None,
            )
        )
        document = json.loads(output.read_text(encoding="utf-8"))
        assert [item["value"] for item in document["debugFlagsInOrder"]] == [
            "-g",
            "-g0",
        ]
        assert document["items"][3]["value"] == "Uno.UI.dll.o"
        rendered_response = output.read_text(encoding="utf-8")
        assert "/safe/public/path" not in rendered_response
        assert fixture_secret not in rendered_response
        assert document["items"][5]["option"] == "--unknown-token"
        assert document["items"][6]["option"] == "--pass-arg"
        assert document["items"][7]["option"] == "-O"

        custom_name = b"name"
        custom_payload = encode_u32_leb(len(custom_name)) + custom_name + b"\x01"
        wasm = root / "fixture.wasm"
        wasm.write_bytes(
            b"\x00asm\x01\x00\x00\x00"
            + b"\x00"
            + encode_u32_leb(len(custom_payload))
            + custom_payload
            + b"\x0a\x01\x00"
        )
        metadata = inspect_wasm(wasm)
        assert metadata["byteSize"] == wasm.stat().st_size
        assert metadata["sections"][0]["customName"] == "name"
        assert metadata["sections"][1]["name"] == "code"

        fake_optimizer = root / "fake-optimizer.py"
        fake_optimizer.write_text(
            "import sys\nsys.exit(0)\n",
            encoding="utf-8",
        )
        events = root / "events.jsonl"
        previous_events = os.environ.get("UNO_AOT_DIAGNOSTICS_EVENTS")
        os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = str(events)
        try:
            return_code = wasm_opt_command(
                argparse.Namespace(
                    real=sys.executable,
                    arguments=[
                        str(fake_optimizer),
                        "--post-emscripten",
                        "-O2",
                        str(wasm),
                        "-o",
                        str(wasm),
                        "-g0",
                    ],
                )
            )
        finally:
            if previous_events is None:
                os.environ.pop("UNO_AOT_DIAGNOSTICS_EVENTS", None)
            else:
                os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = previous_events
        assert return_code == 0
        event_names = {
            json.loads(line)["event"]
            for line in events.read_text(encoding="utf-8").splitlines()
        }
        assert {"wasm-opt-invocation", "wasm-opt-input", "wasm-opt-exit"} <= event_names

        invalid_wasm = root / "invalid.wasm"
        invalid_wasm.write_bytes(b"not-a-wasm-module")
        fake_optimizer.write_text(
            "import sys\nsys.exit(37)\n",
            encoding="utf-8",
        )
        failed_events = root / "failed-events.jsonl"
        os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = str(failed_events)
        try:
            return_code = wasm_opt_command(
                argparse.Namespace(
                    real=sys.executable,
                    arguments=[
                        str(fake_optimizer),
                        "--post-emscripten",
                        "-O2",
                        str(invalid_wasm),
                        f"--unknown-token={fixture_secret}",
                        "-g",
                    ],
                )
            )
        finally:
            if previous_events is None:
                os.environ.pop("UNO_AOT_DIAGNOSTICS_EVENTS", None)
            else:
                os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = previous_events
        assert return_code == 37
        rendered_failed_events = failed_events.read_text(encoding="utf-8")
        assert fixture_secret not in rendered_failed_events
        failed_event_documents = [
            json.loads(line) for line in rendered_failed_events.splitlines()
        ]
        assert any(
            event["event"] == "diagnostic-warning"
            and event.get("scope") == "wasm-opt-metadata"
            for event in failed_event_documents
        )
        assert any(
            event["event"] == "wasm-opt-exit"
            and event.get("returnCode") == 37
            for event in failed_event_documents
        )

        publisher = root / "publisher.py"
        publisher.write_text(
            "from pathlib import Path\n"
            "import sys\n"
            "Path(sys.argv[1]).write_text('started', encoding='utf-8')\n"
            "sys.exit(23)\n",
            encoding="utf-8",
        )
        publisher_marker = root / "publisher-started.txt"
        runner_events = root / "runner-events.jsonl"
        fake_dotnet_root = root / "dotnet"
        fake_wasm_opt = (
            fake_dotnet_root
            / "packs"
            / "Microsoft.NET.Runtime.Emscripten.3.1.56.Sdk.linux-x64"
            / EXPECTED_RUNTIME_PACK_VERSION
            / "tools"
            / "bin"
            / "wasm-opt"
        )
        fake_emcc = fake_wasm_opt.parent.parent / "emscripten" / "emcc"
        fake_wasm_opt.parent.mkdir(parents=True)
        fake_emcc.parent.mkdir(parents=True)
        original_wasm_opt = b"fixture-wasm-opt"
        fake_wasm_opt.write_bytes(original_wasm_opt)
        fake_emcc.write_bytes(b"fixture-emcc")
        (fake_dotnet_root / "sdk" / "10.0.102").mkdir(parents=True)
        previous_events = os.environ.get("UNO_AOT_DIAGNOSTICS_EVENTS")
        try:
            publisher_return_code = run_publisher_command(
                argparse.Namespace(
                    dotnet_root=str(fake_dotnet_root),
                    runtime_pack_version=EXPECTED_RUNTIME_PACK_VERSION,
                    script=str(Path(__file__).resolve()),
                    events=str(runner_events),
                    interval_seconds=0.01,
                    disable_monitor=True,
                    publisher=[
                        sys.executable,
                        str(publisher),
                        str(publisher_marker),
                    ],
                )
            )
        finally:
            if previous_events is None:
                os.environ.pop("UNO_AOT_DIAGNOSTICS_EVENTS", None)
            else:
                os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = previous_events
        assert publisher_return_code == 23
        assert publisher_marker.read_text(encoding="utf-8") == "started"
        assert fake_wasm_opt.read_bytes() == original_wasm_opt
        assert not fake_wasm_opt.with_name(
            "wasm-opt.uno-aot-diagnostics-real"
        ).exists()
        runner_event_documents = [
            json.loads(line)
            for line in runner_events.read_text(encoding="utf-8").splitlines()
        ]
        assert any(
            event["event"] == "diagnostic-warning"
            and event.get("scope") == "wasm-opt-version"
            for event in runner_event_documents
        )
        assert any(
            event["event"] == "publisher-exit"
            and event.get("returnCode") == 23
            for event in runner_event_documents
        )

        cancelled_publisher_marker = root / "cancelled-publisher-started.txt"
        cancellation_events = root / "cancellation-events.jsonl"
        previous_events = os.environ.get("UNO_AOT_DIAGNOSTICS_EVENTS")
        os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = str(cancellation_events)

        def cancel_before_publisher():
            signal.raise_signal(signal.SIGTERM)
            return False

        cancellation_return_code = run_publisher_core(
            [sys.executable, str(publisher), str(cancelled_publisher_marker)],
            cancel_before_publisher,
            lambda: None,
            lambda _root_pid: None,
        )
        assert cancellation_return_code == 128 + signal.SIGTERM
        assert not cancelled_publisher_marker.exists()

        installed_wrapper_marker = root / "cancelled-wrapper-installed.txt"

        def install_then_cancel():
            installed_wrapper_marker.write_text("installed", encoding="utf-8")
            signal.raise_signal(signal.SIGTERM)
            return True

        def restore_after_cancel():
            installed_wrapper_marker.unlink()

        cancellation_return_code = run_publisher_core(
            [sys.executable, str(publisher), str(cancelled_publisher_marker)],
            install_then_cancel,
            restore_after_cancel,
            lambda _root_pid: None,
        )
        assert cancellation_return_code == 128 + signal.SIGTERM
        assert not cancelled_publisher_marker.exists()
        assert not installed_wrapper_marker.exists()
        if previous_events is None:
            os.environ.pop("UNO_AOT_DIAGNOSTICS_EVENTS", None)
        else:
            os.environ["UNO_AOT_DIAGNOSTICS_EVENTS"] = previous_events
        cancellation_event_documents = [
            json.loads(line)
            for line in cancellation_events.read_text(encoding="utf-8").splitlines()
        ]
        assert any(
            event["event"] == "publisher-cancelled-before-start"
            and event.get("returnCode") == 128 + signal.SIGTERM
            and event.get("signal") == signal.SIGTERM
            for event in cancellation_event_documents
        )

        processes = {
            10: {"pid": 10, "ppid": 1, "name": "dotnet", "rssKb": 10, "cpuPercent": 1.0},
            11: {"pid": 11, "ppid": 10, "name": "wasm-opt", "rssKb": 20, "cpuPercent": 2.0},
            12: {"pid": 12, "ppid": 1, "name": "unrelated", "rssKb": 30, "cpuPercent": 3.0},
        }
        assert {item["pid"] for item in descendant_processes(processes, 10)} == {
            10,
            11,
        }
    print(
        "WebAssembly AOT diagnostics self-test passed: "
        "version_probe_nonfatal=true, publisher_exit=23, wrapper_restored=true, "
        "optimizer_metadata_failure_nonfatal=true, optimizer_exit=37, "
        "secret_shaped_options_redacted=true, preflight_cancel_exit=143, "
        "cancelled_publisher_started=false, cancel_cleanup_restored=true."
    )
    return 0


def build_parser():
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)

    response = subparsers.add_parser("response")
    response.add_argument("--path", required=True)
    response.add_argument("--output", required=True)
    response.add_argument("--events")
    response.set_defaults(handler=response_command)

    install = subparsers.add_parser("install-wrapper")
    install.add_argument("--dotnet-root", required=True)
    install.add_argument("--script", required=True)
    install.add_argument(
        "--runtime-pack-version",
        default=EXPECTED_RUNTIME_PACK_VERSION,
    )
    install.set_defaults(handler=install_wrapper_command)

    restore = subparsers.add_parser("restore-wrapper")
    restore.add_argument("--dotnet-root", required=True)
    restore.add_argument(
        "--runtime-pack-version",
        default=EXPECTED_RUNTIME_PACK_VERSION,
    )
    restore.set_defaults(handler=restore_wrapper_command)

    wasm_opt = subparsers.add_parser("wasm-opt")
    wasm_opt.add_argument("--real", required=True)
    wasm_opt.add_argument("arguments", nargs=argparse.REMAINDER)
    wasm_opt.set_defaults(handler=wasm_opt_command)

    run_publisher = subparsers.add_parser("run-publisher")
    run_publisher.add_argument("--dotnet-root", required=True)
    run_publisher.add_argument("--script", required=True)
    run_publisher.add_argument("--events", required=True)
    run_publisher.add_argument(
        "--runtime-pack-version",
        default=EXPECTED_RUNTIME_PACK_VERSION,
    )
    run_publisher.add_argument("--interval-seconds", type=float, default=10.0)
    run_publisher.add_argument("--disable-monitor", action="store_true")
    run_publisher.add_argument("publisher", nargs=argparse.REMAINDER)
    run_publisher.set_defaults(handler=run_publisher_command)

    monitor = subparsers.add_parser("monitor")
    monitor.add_argument("--root-pid", required=True, type=int)
    monitor.add_argument("--output")
    monitor.add_argument("--interval-seconds", type=float, default=10.0)
    monitor.add_argument("--max-processes", type=int, default=24)
    monitor.set_defaults(handler=monitor_command)

    self_test = subparsers.add_parser("self-test")
    self_test.set_defaults(handler=self_test_command)
    return parser


def main():
    args = build_parser().parse_args()
    try:
        return args.handler(args)
    except Exception as error:
        emit(
            "diagnostic-error",
            status="unavailable",
            errorType=type(error).__name__,
        )
        return 1


if __name__ == "__main__":
    sys.exit(main())
