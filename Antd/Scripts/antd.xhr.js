var _requests = [];

var _abortAllRequests = function () {
    $(_requests).each(function (i, xhr) {
        xhr.abort();
    });
    _requests = [];
}

$(window).on("beforeunload", function () {
    _abortAllRequests();
});

function AbortAllAjaxRequests() {
    _abortAllRequests();
}