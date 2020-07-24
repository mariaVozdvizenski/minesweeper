var controlPressed = false;
$(document).on("keydown", function (event) {
    if (event.ctrlKey)
        controlPressed = true;
});
$(document).on("keyup", function () {
    controlPressed = false;
});
function makeHTTPRequest(url) {
    $.ajax({
        url: url,
        type: "POST",
        success: function (result) {
            console.log(location.href);
            location.reload(true);
        },
        error: function (error) {
            console.log(error);
        }
    });
}
function cellClick(x, y, id) {
    var baseUrl = "https://localhost:5001/PlayGame?x=" + x + "&y=" + y + "&id=" + id;
    var url = baseUrl + "&handler=move";
    makeHTTPRequest(url);
}
$('body').attr('oncontextmenu', 'return false;');
function cellFlag(x, y, id) {
    console.log(x, y, id);
    var baseUrl = "https://localhost:5001/PlayGame?x=" + x + "&y=" + y + "&id=" + id;
    var url = baseUrl + "&handler=flag";
    makeHTTPRequest(url);
}
function mouseOver(cell) {
    cell.classList.add("mouse-over");
}
function mouseOut(cell) {
    cell.classList.remove("mouse-over");
}
//# sourceMappingURL=minesweeper.js.map