
var CurrentlyHighlightedMenuItem = "";

function MenuHighlight(name) {

    if (CurrentlyHighlightedMenuItem != "") {

        var OldItem = document.getElementById('Item' + CurrentlyHighlightedMenuItem);
        if (OldItem != null)
            OldItem.style.backgroundColor = "";

        var OldMenu = document.getElementById('Menu' + CurrentlyHighlightedMenuItem);
        if (OldMenu != null)
            OldMenu.style.display = "none";

    }

    var NewItem = document.getElementById('Item' + name);
    if (NewItem != null)
        NewItem.style.backgroundColor = "rgb(206, 137, 0)";

    var NewMenu = document.getElementById('Menu' + name);
    if (NewMenu != null)
        NewMenu.style.display = "block";

    CurrentlyHighlightedMenuItem = name;

    if (history && history.pushState) {
        history.pushState(null, null, '/' + name.toLowerCase() + '/index.shtml');
    }

}


function SendJSON(HTTPVerb, URI, APIKey, Data, OnSuccess, OnError) {

    var ajax = new XMLHttpRequest();
    ajax.open(HTTPVerb, URI, true); // , user, password);
    ajax.setRequestHeader("Accept",       "application/json; charset=UTF-8");
    ajax.setRequestHeader("Content-Type", "application/json; charset=UTF-8");

    if (APIKey != null)
        ajax.setRequestHeader("APIKey", APIKey);

    ajax.onreadystatechange = function () {

        // 0 UNSENT | 1 OPENED | 2 HEADERS_RECEIVED | 3 LOADING | 4 DONE
        if (this.readyState == 4) {

            // Ok
            if (this.status >= 100 && this.status < 300) {

                //alert(ajax.getAllResponseHeaders());
                //alert(ajax.getResponseHeader("Date"));
                //alert(ajax.getResponseHeader("Cache-control"));
                //alert(ajax.getResponseHeader("ETag"));

                if (OnSuccess && typeof OnSuccess === 'function')
                    OnSuccess(this.status, ajax.responseText);

            }

            else
                if (OnError && typeof OnError === 'function')
                    OnError(this.status, this.statusText, ajax.responseText);

        }

    }

    if (Data != null)
        ajax.send(JSON.stringify(Data));
    else
        ajax.send();

}
