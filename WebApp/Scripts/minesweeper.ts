let controlPressed: boolean = false;

$(document).on("keydown", function(event){
    if (event.ctrlKey)
        controlPressed = true;
});

$(document).on("keyup", function(){
    controlPressed = false;
});

function makeHTTPRequest(url: string){
    $.ajax({
        url: url,
        type: "POST",
        success: function (result) {
            console.log(location.href)
            location.reload(true);
        },
        error: function (error) {
                console.log(error);}
    });
}

function cellClick(x: number, y: number, id: number){
    const baseUrl = `https://localhost:5001/PlayGame?x=${x}&y=${y}&id=${id}`; 
    let url = baseUrl + "&handler=move"
    makeHTTPRequest(url);
}

$('body').attr('oncontextmenu','return false;');

function cellFlag(x: number, y: number, id: number){
    console.log(x, y, id);
    const baseUrl = `https://localhost:5001/PlayGame?x=${x}&y=${y}&id=${id}`;
    let url = baseUrl + "&handler=flag"
    makeHTTPRequest(url);
}

function mouseOver(cell: HTMLTableCellElement) {
    cell.classList.add("mouse-over")
}

function mouseOut(cell: HTMLTableCellElement) {
    cell.classList.remove("mouse-over")
}