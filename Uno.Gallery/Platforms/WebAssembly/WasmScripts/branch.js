(function () {
    const head = document.getElementsByTagName("head")[0];

    const script = document.createElement("script");
    script.type = "text/javascript";
    script.innerHTML =
`
// load Branch
(function (b, r, a, n, c, h, _, s, d, k) {
    if (!b[n] || !b[n]._q) {
        for (; s < _.length; ) c(h, _[s++]);
        d = r.createElement(a);
        d.async = 1;
        d.src = "https://cdn.branch.io/branch-latest.min.js";
        k = r.getElementsByTagName(a)[0];
        k.parentNode.insertBefore(d, k);
        b[n] = h;
    }
})(
    window,
    document,
    "script",
    "branch",
    function (b, r) {
        b[r] = function () {
            b._q.push([r, arguments]);
        };
    },
    { _q: [], _v: 1 },
    "addListener applyCode autoAppIndex banner closeBanner closeJourney creditHistory credits data deepview deepviewCta first getCode init link logout redeem referrals removeListener sendSMS setBranchViewData setIdentity track validateCode trackCommerceEvent logEvent disableTracking".split(
        " "
    ),
    0
);
// init Branch
branch.init("key_live_eg8jUSrrIb5TEDaviMJkXbccCDi0y7wn", {}, (err, data) => {
    if (data?.data_parsed["$deeplink_path"]) {
        const onDeepLink = Module.mono_bind_static_method("[Uno.Gallery.Wasm] Uno.Gallery.App:TryNavigateToLaunchSample");
        onDeepLink(data.data_parsed["$deeplink_path"], data.data_parsed["$design"] ?? "");
    }
});
`;
    head.appendChild(script);
})();