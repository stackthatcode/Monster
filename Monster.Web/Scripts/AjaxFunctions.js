
var LogicAuto = LogicAuto || {};

LogicAuto.StandardErrorMessage = "Something went wrong. Please refresh your browser and try again.";
LogicAuto.Http403ErrorMessage = "You are not authorized to view that resource.";
LogicAuto.Http401ErrorMessage = "You appear to be currently logged out of EWC.";

LogicAuto.ErrorPopup = function (message) {
    alert(message || LogicAuto.StandardErrorMessage);
}

LogicAuto.AjaxSettings = function (modal) {
    return {
        BaseUrl: LogicAuto.BaseUrl,
        Timeout: 120000,    // Two minutes for development
        UseSpinner: true,
        Modal: modal || false,
    };
};

LogicAuto.ShowLoading = function () {
    $("#spinner-layer").show();
};

LogicAuto.HideLoading = function () {
    $("#spinner-layer").hide();
};


LogicAuto.AjaxGet = function (url) {
    flow.exec(
        function () {
            var ajax = new LogicAuto.Ajax();
            ajax.HttpGet(url, this);
        },
        function (data) {
            return data;
        });
    };

LogicAuto.Ajax = function (settings) {
    var self = this;

    self.Settings = settings || new LogicAuto.AjaxSettings();

    self.ErrorCallback = function (jqXHR, textStatus, errorThrown) {        
        self.HideLoading();

        if (jqXHR.status != 0 || textStatus == "timeout") {
            var message = LogicAuto.StandardErrorMessage;

            if (jqXHR.status == 401) {
                alert(LogicAuto.Http401ErrorMessage);

                window.location.href =
                    PushConfig.BaseUrl + "?ReturnUrl=" +
                        encodeURIComponent(window.location.href);

                return;
            }

            if (jqXHR.status == 403) {
                message = LogicAuto.Http403ErrorMessage
            }
            
            if (self.Settings.Modal) {
                // Avoid nested modals and use browser alert when one is present, already
                alert(message);
            } else {
                LogicAuto.ErrorPopup(message);
            }
        }
    };

    self.AjaxUniqueUrl = function(url) {
        var dateStampParam = "_=" + new Date().getTime();
        return url.indexOf("?") == -1 ? url + "?" + dateStampParam : url + "&" + dateStampParam;
    };

    self.HttpGet = function (url, successFunc) {
        flow.exec(
            function () {
                self.ShowLoading();
                $.ajax({
                    type: 'GET',
                    url: self.Settings.BaseUrl + self.AjaxUniqueUrl(url),
                    timeout: self.Settings.Timeout,
                    error: self.ErrorCallback,
                    success: this
                });
            },
            function (data, textStatus, jqXHR) {
                self.HideLoading();

                if (successFunc) {
                    successFunc(data, textStatus, jqXHR);
                }
            }
        );
    };

    self.HttpPost = function (url, data, successFunc) {
        var token = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
        
        flow.exec(
            function () {
                self.ShowLoading();

                $.ajax({
                    type: 'POST',
                    dataType: 'json',
                    beforeSend: function (request) {
                        request.setRequestHeader("__RequestVerificationToken", token);
                    },
                    contentType: 'application/json; charset=utf-8',
                    url: self.Settings.BaseUrl + self.AjaxUniqueUrl(url),
                    data: JSON.stringify(data),
                    timeout: self.Settings.Timeout,
                    error: self.ErrorCallback,
                    success: this
                });
            },
            function (data, textStatus, jqXHR) {
                self.HideLoading();
                if (successFunc) {
                    successFunc(data, textStatus, jqXHR);
                }
            }
        );
    };

    self.ShowLoading = function () {
        if (self.Settings.UseSpinner) {
            LogicAuto.ShowLoading();
        }
    };

    self.HideLoading = function () {
        if (self.Settings.UseSpinner) {
            LogicAuto.HideLoading();
        }
    };
};

