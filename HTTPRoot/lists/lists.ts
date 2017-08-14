///<reference path="../libs/date.format.ts" />
///<reference path="../libs/sse.d.ts" />
///<reference path="../libs/EventSource.ts" />


function StartListsSSE() {

    MenuHighlight('Lists');
    var ConnectionColors = {};
    var StreamFilterPattern = <HTMLInputElement>document.getElementById('StreamFilterDiv').getElementsByTagName('input')[0];

    StreamFilterPattern.onchange = function () {

        var AllLogLines = document.getElementById('EventsDiv').getElementsByClassName('LogLine');

        for (var i = 0; i < AllLogLines.length; i++) {
            if (AllLogLines[i].innerHTML.indexOf(StreamFilterPattern.value) > -1)
                (<HTMLDivElement>AllLogLines[i]).style.display = 'table-row';
            else
                (<HTMLDivElement>AllLogLines[i]).style.display = 'none';
        }

    }

}
