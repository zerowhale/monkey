var ExternalLog = {
    window: null,
    show: function () {
        this.window = window.open("/external-log", "test", "menubar=no,location=yes,resizable=yes,scrollbars=yes,status=no,width=400,height=600");
        return this.window;
    },

    log: function (html) {
        var b = $(this.window.document.body);

        b.append("<div>" + html + "</div>");
        
    }
}   