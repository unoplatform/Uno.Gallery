#!/usr/bin/env python3
"""Checks an NUnit results file from a runtime tests lane, and prints its counts.

Shared by the desktop, WebAssembly, Android and iOS runtime tests scripts. Fails when the file can't be read,
contains no test cases (a crashed app or a broken filter would otherwise look green), or contains a test case
that neither passed nor was skipped.
"""
import sys
import xml.etree.ElementTree as ET
from collections import Counter

if len(sys.argv) != 2:
    raise SystemExit("usage: validate-runtime-test-results.py <results.xml>")

results_path = sys.argv[1]

try:
    cases = ET.parse(results_path).findall(".//test-case")
except (ET.ParseError, OSError) as exc:
    raise SystemExit(f"ERROR: Unable to read the runtime tests results from {results_path}: {exc}")

if not cases:
    raise SystemExit(f"ERROR: The runtime tests results in {results_path} contain no test cases.")

counts = Counter(case.get("result") or "Unknown" for case in cases)
print(f"Runtime tests: {len(cases)} total, " + ", ".join(f"{count} {result.lower()}" for result, count in sorted(counts.items())))

failed = [case for case in cases if case.get("result") not in ("Passed", "Skipped")]
for case in failed:
    print(f" - {case.get('fullname') or case.get('name')}: {case.get('result')}")

sys.exit(1 if failed else 0)
