var lineCap_butt = "butt";
var lineCap_round = "round";
var lineCap_square = "square";
var patternParam_Repeat = "repeat";

/*
Draw
*/

function DrawLine(context, startX, startY, endX, endY) {
    DrawLine(context,startX,startY,endX,endY,1,"#000000",lineCap_butt);
}

function DrawLine(context, startX, startY, endX, endY, lineWidth, strokeStyle, lineCap) {
    context.beginPath();
    context.moveTo(startX, startY);
    context.lineTo(endX, endY);
    context.lineWidth = lineWidth;
    context.strokeStyle = strokeStyle;
    context.lineCap = lineCap;
    context.stroke();
}

function DrawArc(context, startX, startY, radius, startAngle, endAngle, lineWidth, strokeStyle) {
    context.beginPath();
    context.arc(startX, startY, radius, startAngle, endAngle, false);
    context.lineWidth = lineWidth;
    context.strokeStyle = strokeStyle;
    context.stroke();
}

function DrawRect(context, startX, startY, width, height, fillstyle, lineWidth, strokeStyle) {
    context.beginPath();
    context.rect(startX, startY, width, height);
    context.fillStyle = fillstyle;
    context.fill();
    context.lineWidth = lineWidth;
    context.strokeStyle = strokeStyle;
    context.stroke();
}

function DrawCircle(context, centerX, centerY, radius) {
    DrawArc(context, centerX, centerY, radius, 0, 2 * Math.PI);
}

function DrawImage(context, src, startX, startY,width,height) {
    var imageObj = new Image();
    imageObj.onload = function () {
        context.drawImage(imageObj, startX, startY,width,height);
    };
    imageObj.src = src;
}

function DrawText(context, font, text, startX, startY) {
    context.font = font;
    context.fillText(text, startX, startY);
}

function CreatePatternWithImage(context,imageObj, param) {
    return context.createPattern(imageObj, param);
}

function ClearAll(context,canvas) {
    context.clearRect(0, 0, canvas.width, canvas.height);
}
