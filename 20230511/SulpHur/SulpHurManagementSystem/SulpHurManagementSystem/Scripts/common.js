
function addOnLoadHandler(handler) {
    var oldLoad = window.onload;
    window.onload = function () {
        handler();
        oldLoad();
    }
}

function init() {
    regHotKey();
}

//system key process
function regHotKey() {
    //define hotkeys
    objShieldKeyMap = {
        KEYCD_13: 'ENTER',
        KEYCD_27: 'ESC',
        KEYCD_112: 'F1',
        KEYCD_113: 'F2',
        KEYCD_114: 'F3',
        KEYCD_115: 'F4',
        KEYCD_116: 'F5',
        KEYCD_117: 'F6',
        KEYCD_118: 'F7',
        KEYCD_119: 'F8',
        KEYCD_120: 'F9',
        KEYCD_121: 'F10',
        KEYCD_122: 'F11',
        KEYCD_123: 'F12'
    };
    //hotkeys mapped on a specfic page
    objHotKeyMap = {};
    strHotKeys = '';

    //hotkey in attribute
    var $hotkeyElement = $('[hotkey]');
    for (var index in $hotkeyElement.toArray()) { 
        var strHotKeyAttrValue = $hotkeyElement[index].getAttribute('hotkey');
        if (strHotKeyAttrValue) {
            var strHotKey = strHotKeyAttrValue.toUpperCase();
            if (objHotKeyMap[strHotKey]) {
                objHotKeyMap[strHotKey].push($hotkeyElement[index]);
            } else {
                objHotKeyMap[strHotKey] = new Array();
                objHotKeyMap[strHotKey].push($hotkeyElement[index]);
            }
            strHotKeys = strHotKeys + ',' + strHotKey;
        }
    }

    //hotkey in name
    for (var i = 0; i < document.forms.length; i++) {
        var form = document.forms[i];
        for (var j = 0; j < form.elements.length; j++) {
            var e = form.elements[j];
            if ((e.tagName.toLowerCase() == 'button') ||
				((e.tagName.toLowerCase() == 'input') &&
					(e.type.toLowerCase() == 'submit' ||
					e.type.toLowerCase() == 'button'))) {
                var haveHotKey = /^[\w\s]+\(F1[0-2]\)$|^[\w\s]+\(F[1-9]\)$|^[\w\s]+\(ESC\)$/.test(e.value.toUpperCase());
                if (haveHotKey) {
                    var arrHotkey = e.value.toUpperCase().match(/F1[0-2]|F[1-9]|ESC/);
                    if (objHotKeyMap[arrHotkey[0]]) {
                        objHotKeyMap[arrHotkey[0]].push(e);
                    } else {
                        objHotKeyMap[arrHotkey[0]] = new Array();
                        objHotKeyMap[arrHotkey[0]].push(e);
                    }
                    strHotKeys = strHotKeys + ',' + arrHotkey[0];
                }
            }
        }
    }
    document.onkeydown = keyboardListener;
}
function keyboardListener() {
    var keyCode = event.keyCode;
    var keyName = objShieldKeyMap['KEYCD_' + keyCode];
    if (keyName == null ||
        (keyName == 'ENTER' && document.activeElement.type == 'textarea')) {
        //allow textarea to change line
    } else {
        objBtnArr = objHotKeyMap[keyName.toUpperCase()];
        if (objBtnArr) {
            var objVisibleBtnArr = new Array();
            for (var index in objBtnArr) {
                if ($(objBtnArr[index]).is(':visible')) {
                    objVisibleBtnArr.push(objBtnArr[index]);
                }
            }
            if (objVisibleBtnArr.length > 0) {
                objVisibleBtnArr.sort(function (a, b) {
                    return $(b).zIndex() - $(a).zIndex();
                });
                objVisibleBtnArr[0].click();
            }
        }

        event.keyCode = 0;
        event.returnValue = false;
        return false;
    }
}

function ResizeImage(ImgD, iwidth, iheight) {
    var image = new Image();
    image.src = ImgD.src;
    if (image.width > 0 && image.height > 0) {
        if (image.width / image.height >= iwidth / iheight) {
            if (image.width > iwidth) {
                ImgD.width = iwidth;
                ImgD.height = (image.height * iwidth) / image.width;
            } else {
                ImgD.width = image.width;
                ImgD.height = image.height;
            }
        } else {
            if (image.height > iheight) {
                ImgD.height = iheight;
                ImgD.width = (image.width * iheight) / image.height;
            } else {
                ImgD.width = image.width;
                ImgD.height = image.height;
            }
        }
    }
}

String.prototype.trim = function()  
{
    return this.replace(/(^\s*)|(\s*$)/g, "");
}
String.prototype.contains = function (it) {
    return this.indexOf(it) != -1;
}
var escapeUserInput = function (str) {
    return (str + '').replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");
};
