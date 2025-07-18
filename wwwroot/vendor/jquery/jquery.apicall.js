(function ($) {
  $.apiCall = function (options) {
    const {
      url,
      method = null,
      data = null,
      headers = {},
      token = null,
      loaderSelector = null,
      onSuccess = function () {},
      onError = function (xhr, status, error) {
        console.error("API Error:", error);
        if (xhr.responseJSON?.message) {
          alert("Error: " + xhr.responseJSON.message);
        } else {
          alert("Terjadi kesalahan saat menghubungi server.");
        }
      },
      beforeSend = function () {},
      complete = function () {}
    } = options;

    const finalHeaders = {
      ...headers
    };

    if (token) {
      finalHeaders["Authorization"] = "Bearer " + token;
    }

    const isGet = method.toUpperCase() === "GET";
    const requestData = isGet ? data : JSON.stringify(data);

    $.ajax({
      url: url,
      method: method,
      data: requestData,
      headers: finalHeaders,
      contentType: isGet
        ? "application/x-www-form-urlencoded; charset=UTF-8"
        : "application/json",
      beforeSend: function () {
        if (loaderSelector) $(loaderSelector).show();
        beforeSend();
      },
      success: function (response) {
        onSuccess(response);
      },
      error: function (xhr, status, error) {
        onError(xhr, status, error);
      },
      complete: function () {
        if (loaderSelector) $(loaderSelector).hide();
        complete();
      }
    });
  };
})(jQuery);