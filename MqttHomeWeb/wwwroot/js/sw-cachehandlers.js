// custom handler for online-only content
toolbox.networkOnlyCustom = function (req, vals, opts) {
    return toolbox.networkOnly(req, vals, opts)
        .catch(function (error) {
            if (req.method === "GET" && req.headers.get("accept").includes("text/html"))
                return toolbox.cacheOnly(new Request("/home/offline"), vals, opts);
            throw error;
        });
};

// custom handler for missing network-first content
toolbox.networkFirstCustom = function (req, vals, opts) {
    return toolbox.networkFirst(req, vals, opts)
        .catch(function (error) {
            if (req.method === "GET" && req.headers.get("accept").includes("text/html"))
                return toolbox.cacheOnly(new Request("/home/offline"), vals, opts);
            throw error;
        });
};

toolbox.logoutHandler = function(req, vals, opts) {
    return toolbox.networkFirst(req, vals, opts)
        .catch(function(error) {
            return toolbox.cacheOnly(new Request("/home/offlinelogout"), vals, opts);
        });
};