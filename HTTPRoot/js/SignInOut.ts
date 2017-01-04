var SignInUser    = "";
var HTTPCookieId  = "OpenDataSocial";


function HideElement(DivName) {

    var div = document.querySelector(DivName);
    div.style.display = "none";

    return div;

}

function ShowElement(DivName) {
    return ShowElement2(DivName, "inline-block");
}

function ShowElement2(DivName, displaymode) {

    if (displaymode == undefined)
        displaymode = "inline-block";

    var div = document.querySelector(DivName);
    div.style.display = displaymode;

    return div;

}


function SendJSON(HTTPVerb, URI, Data, OnSuccess, OnError) {

    var ajax = new XMLHttpRequest();
    ajax.open(HTTPVerb, URI, true);
    ajax.setRequestHeader("Accept",       "application/json; charset=UTF-8");
    ajax.setRequestHeader("Content-Type", "application/json; charset=UTF-8");

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

    // send the data as JSON
    ajax.send(JSON.stringify(Data));

  //  ajax.send("{ \"username\": \"ahzf\" }");

}



function GetCookie(CookieName, OnSucess, OnFailure) {

    var results = document.cookie.match('(^|;) ?' + CookieName + '=([^;]*)(;|$)');

    if (results == null) {
        OnFailure();
        return null;
    }

    if (OnSucess == undefined)
        return results[2];

    OnSucess(results[2]);

}

function DeleteCookie(CookieName, Path) {
    DeleteCookie2(CookieName, "/");
}

function DeleteCookie2(CookieName, Path) {

    var CookieDateTime = new Date();
    CookieDateTime.setTime(CookieDateTime.getTime() - 86400000); // 1 day

    if (Path == undefined)
        Path = "/";

    document.cookie = CookieName += "=; expires=" + CookieDateTime.toUTCString() + "; Path=" + Path;

}



function SignIn() {

    var SignInPanel  = document.querySelector('#login');
    var Username     = (<HTMLInputElement> SignInPanel.querySelector('#_username')).  value;
    //var Realm        = (<HTMLInputElement> SignInPanel.querySelector('#_realm')).     value;
    var Realm        = "";
    var Password     = (<HTMLInputElement> SignInPanel.querySelector('#_password')).  value;
    var RememberMe   = (<HTMLInputElement> SignInPanel.querySelector('#_rememberme')).checked;

    var SignInErrors = <HTMLElement> SignInPanel.querySelector('#errors');
    SignInErrors.style.display = "none";
    SignInErrors.innerText     = "";

    SendJSON("AUTH",
             "/users/" + Username,
             {
                 "realm":      Realm,
                 "password":   Password,
                 "rememberme": RememberMe
             },

             function (HTTPStatus, ResponseText) {
                 //(<HTMLFormElement> document.querySelector('#loginform')).submit();
                 location.href = "/";
             },

             function (HTTPStatus, StatusText, ResponseText) {

                 SignInErrors.style.display = "block";
                 SignInErrors.innerText = JSON.parse(ResponseText).description;

             });

            }

function checkSignedIn() {

    var SocialOpenDataCookie = GetCookie(HTTPCookieId,
                                         cookie => {

                                             ShowElement('#profile');
                                             ShowElement('#maintenance');
                                             HideElement('#signin');
                                             ShowElement('#signout');

                                             // Crumbs are base64 encoded!
                                             var crumbs = cookie.split(":").forEach(function (crumbs) {

                                                 if (crumbs.startsWith("username"))
                                                     SignInUser = atob(crumbs.split("=")[1]);

                                                 if (crumbs.startsWith("name"))
                                                     (<HTMLElement> document.querySelector('#username')).innerText = atob(crumbs.split("=")[1]);

                                             });

                                         },

                                         () => {

                                             HideElement('#profile');
                                             HideElement('#maintenance');
                                             ShowElement('#signin');
                                             HideElement('#signout');

                                         }

                                        );

}

function SignOut() {

    SendJSON("DEAUTH",
             "/users",
             "",

             function (HTTPStatus, ResponseText) {
             },

             function (HTTPStatus, StatusText, ResponseText) {
             });

    DeleteCookie(HTTPCookieId, "/");

    location.href = "/login.html";

}

