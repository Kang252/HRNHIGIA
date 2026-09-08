'use strict';
angular.module('hrmApplication.directives', [])
    .directive('appVersion', [
        'version', function (version) {
            return function (scope, elm, attrs) {
                elm.text(version);
            };
        }
    ])
    .directive('ngHrmHidden', [
        function () {
            return {
                //attribute or element
                restrict: 'AE',
                scope: {},
                replace: true,
                require: 'ngModel',
                link: function ($scope, elem, attr, ngModel) {
                    $scope.$watch(ngModel, function (nv) {
                        elem.val(nv);
                    });
                    elem.change(function () {
                        //bind the change event to hidden input
                        $scope.$apply(function () {
                            ngModel.$setViewValue(elem.val());
                        });
                    });
                }
            };
        }
    ])
    .directive('numbersOnly', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attr, ngModelCtrl) {
                function fromUser(text) {
                    if (text) {
                        var transformedInput = text.replace(/[^0-9]/g, '');
                        if (transformedInput !== text) {
                            ngModelCtrl.$setViewValue(transformedInput);
                            ngModelCtrl.$render();
                        }
                        return transformedInput;
                    }
                    return undefined;
                }
                ngModelCtrl.$parsers.push(fromUser);
            }
        };
    })
    .directive('numberFormat', ['$filter', '$timeout', function ($filter, $timeout) {
        return {
            require: "?ngModel",
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) {
                    return;
                }
                function minVal() {
                    return parseFloat(attrs.min);
                }
                function maxVal() {
                    return parseFloat(attrs.max);
                }
                function getMaxlen() {
                    var res = parseInt(attrs.maxlengthOrigin);
                    function commaCount(val) {
                        var count = 0;
                        var absVal = Math.abs(val);
                        while ((absVal /= 1000) >= 1) count++;
                        return count + (val < 0 ? 1 : 0);
                    }
                    var commaCountMin = commaCount(minVal());
                    var commaCountMax = commaCount(maxVal());
                    res += commaCountMin > commaCountMax ? commaCountMin : commaCountMax;
                    return res;
                }
                attrs.$set("maxlengthOrigin", attrs.maxlength);
                var maxlen = getMaxlen();
                var currMaxVal = minVal();
                var currMinVal = maxVal();
                attrs.$set("maxlength", maxlen);
                attrs.$observe("max", function (newMaxVal) {
                    if (currMaxVal != newMaxVal) {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen);
                        ctrl.$validate();
                        currMaxVal = newMaxVal;
                    }
                });
                attrs.$observe("min", function (newMinVal) {
                    if (currMinVal != newMinVal) {
                        ctrl.$validate();
                        currMinVal = newMinVal;
                    }
                });
                var selectionStart = 0;
                var newNumber = true;
                var dotPressed = false;
                var dashPressed = false;
                var oldCommaCount = 0;
                elem.bind("keydown keypress", function (event) {
                    // determine if keypress will generate new char in model
                    newNumber = (event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105) || event.which == 190 || event.which == 110;
                    if ((event.which == 8 || event.which == 46) && elem[0].selectionStart != elem[0].selectionEnd) {
                        // range deletion
                        // memorize the selection start and let the default behaviour through.
                        selectionStart = elem[0].selectionStart;
                        return;
                    }
                    //minus sign
                    if (event.which == 189 || event.which == 109) {
                        if (elem[0].selectionStart == 0) {
                            // in IE, pressing 189/109 release both 189/109 AND 45 (insert) sequentially
                            // raise this flag to handle the case when insert is not intended
                            dashPressed = true;
                        }
                        else {
                            event.preventDefault();
                            return;
                        }
                    }
                    // handle the dash key
                    if (dashPressed && event.which == 45) {
                        dashPressed = false;
                        newNumber = true;
                    }
                    if ((event.which == 8 && elem[0].selectionStart == elem[0].selectionEnd && elem[0].selectionStart < elem.val().length && elem.val()[elem[0].selectionStart - 1] == ".") ||
                        (event.which == 46 && elem[0].selectionStart == elem[0].selectionEnd && elem[0].selectionStart < elem.val().length - 1 && elem.val()[elem[0].selectionStart] == ".")) {
                        // do not allow deletion of the period "." if there is a decimal part.
                        // to delete the period, delete the decimal part first
                        event.preventDefault();
                        return;
                    }
                    if (event.which == 8 && elem[0].selectionStart > 0) {
                        // take over the default behaviour and emulate backspace key, taking into account the "," char
                        event.preventDefault();
                        // if the char to be deletedis "," - delete the char before it instead
                        var selectionMove = (elem.val()[elem[0].selectionStart - 1] == "," ? 1 : 0);
                        selectionStart = elem[0].selectionStart - 1;
                        var eVal = elem.val();
                        elem.val(eVal.slice(0, selectionStart - selectionMove) + eVal.slice(selectionStart + 1 - selectionMove));
                    }
                    else if (event.which == 46 && elem[0].selectionEnd <= elem.val().length) {
                        // emulate delete key
                        if (dotPressed) {
                            // typing "." will also trigger a delete key. In that case, we let the default behaviour
                            dotPressed = false;
                        } else {
                            // else, take over the default behaviour and emulate delete key taking into account the "," char
                            event.preventDefault();
                            // if the char to be deleted is "," - delete the char after it instead
                            var selectionMove = (elem.val()[elem[0].selectionStart] == "," ? 1 : 0);
                            selectionStart = elem[0].selectionStart;
                            var eVal = elem.val();
                            elem.val(eVal.slice(0, selectionStart + selectionMove) + eVal.slice(selectionStart + 1 + selectionMove));
                        }
                    }
                    else {
                        selectionStart = elem[0].selectionStart;
                    }
                    // no . if there is already a . or cursor leave more than 2 decimal digits
                    if (attrs.isDecimal != undefined && (event.which == 190 || event.which == 110) && (elem.val().indexOf(".") > 0 || elem[0].selectionStart < elem.val().length - 2)) {
                        newNumber = false;
                        event.preventDefault();
                    }
                    if ((attrs.isDecimal == undefined && (event.which == 190 || event.which == 110)) || // no . if decimal is not allowed
                        //(event.which >= 32 && event.which <= 47 && event.which != 46 && event.which != 45) ||
                        (event.which >= 58 && event.which <= 95) ||
                        (event.which >= 106 && event.which <= 107) ||
                        ((minVal() >= 0 || elem[0].selectionStart != 0) && event.which == 109) ||
                        ((minVal() >= 0 || elem[0].selectionStart != 0) && event.which == 189) ||
                        event.which == 32 ||
                        event.which == 111 ||
                        (event.which >= 124 && event.which != 190 && event.which != 189)) {
                        event.preventDefault();
                        newNumber = false;
                    } else if (attrs.isDecimal != undefined) {
                        var dot = elem.val().indexOf(".");
                        if (dot > 0 && dot == elem.val().length - 3 // there is a dot and there is 2 number following the dot
                            && dot < elem[0].selectionStart // the cursor is placed after the dot
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        }
                        if (dot > 0 && dot >= attrs.maxlength - 3  // there is a dot and it is at the maximum pos
                            && dot > elem[0].selectionStart // the cursor is placed before the dot
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        } else if (dot < 0 && elem.val().length >= attrs.maxlength - 3 // there is no dot, and user reached maximum char count for the integer part
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        }
                    }
                    // count the number of comma in old string
                    oldCommaCount = (elem.val().substring(0, selectionStart).match(/,/g) || []).length;
                    if (newNumber) {
                        selectionStart++;
                    }
                    //in IE, pressing . release 190/110 AND 46 sequentially
                    if (event.which == 190 || event.which == 110) {
                        dotPressed = true;
                    }
                });
                elem.bind("focus", function (e) {
                    if ((elem.val() == "0" || elem.val() == "0.00") && attrs.readonly == undefined) {
                        scope.$eval(attrs.ngModel + "=''");
                        elem.val("");
                    }
                    elem.select();
                });
                elem.bind("blur", function (e) {
                    if (elem.val() == "" || elem.val() == "." || elem.val() == "-" || isNaN(ctrl.$modelValue)) {
                        var defaultVal = (attrs.isDecimal == undefined ? "0" : "0.00");
                        scope.$eval(attrs.ngModel + "='" + defaultVal + "'");
                        $timeout(function () {
                            elem.val(defaultVal);
                        });
                    } else if (attrs.isDecimal != undefined) {
                        var val = $filter("number")(ctrl.$modelValue, 2);
                        scope.$eval(attrs.ngModel + "='" + val.replace(/,/g, "") + "'");
                        $timeout(function () {
                            elem.val(val);
                        });
                    }
                    ctrl.$validate();
                });
                // Bind to the mouseup event of the input textbox. 
                elem.bind('mouseup', function () {
                    // Get the old value (before click) and return if it's already empty
                    // as there's nothing to do.
                    var $input = $(this), oldValue = $input.val();
                    if (oldValue === '') return;
                    // Check new value after click, and if it's now empty it means the
                    // clear button was clicked. Manually trigger element's change() event.
                    $timeout(function () {
                        var newValue = $input.val();
                        if (newValue === '') {
                            // return default value
                            var defaultVal = (attrs.isDecimal == undefined ? "0" : "0.00");
                            scope.$eval(attrs.ngModel + "='" + defaultVal + "'");
                            elem.val(defaultVal);
                            angular.element($input).change();
                            ctrl.$validate();
                        }
                    }, 1);
                });
                ctrl.$formatters.unshift(function () {
                    return $filter("number")(ctrl.$modelValue, attrs.isDecimal != undefined ? 2 : 0);
                });
                ctrl.$parsers.unshift(function (viewValue) {
                    var removeCharPattern = "[^0-9"
                        + (attrs.isDecimal != undefined ? "\\." : "")
                        + (parseFloat(attrs.min) < 0 ? "\\-" : "")
                        + "]";
                    var plainNumber = viewValue.replace(new RegExp(removeCharPattern, "g"), "");
                    var formattedNumber;
                    if (plainNumber != "" && plainNumber != "-" && !isNaN(plainNumber)) {
                        var suffix = "";
                        if (plainNumber[plainNumber.length - 1] == "0") {
                            if (plainNumber[plainNumber.length - 2] == ".") {
                                suffix = ".0";
                            } else if (plainNumber[plainNumber.length - 3] == "." && plainNumber[plainNumber.length - 2] == "0") {
                                suffix = ".00";
                            } else if (plainNumber[plainNumber.length - 3] == ".") {
                                suffix = "0";
                            }
                        }
                        formattedNumber = $filter("number")(plainNumber) + suffix;
                    }
                    else
                        formattedNumber = plainNumber;
                    if (plainNumber[plainNumber.length - 1] == '.') formattedNumber += ".";
                    elem.val(formattedNumber);
                    if (minVal() < 0 && formattedNumber[0] != '-') {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen - 1);
                    } else {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen);
                    }
                    // fix for ctFormC_IAUtilNorm
                    if (attrs.id == "ctFormC_IAUtilNorm") {
                        attrs.$set("maxlength", 14);
                    }
                    ctrl.$touched = true;
                    var newCommaCount = (formattedNumber.substring(0, selectionStart).match(/,/g) || []).length;
                    if (oldCommaCount < newCommaCount) {
                        selectionStart++;
                    } else if (oldCommaCount > newCommaCount) {
                        selectionStart--;
                    }
                    elem[0].selectionStart = selectionStart;
                    elem[0].selectionEnd = selectionStart;
                    return plainNumber;
                });
                ctrl.$validators.invalid = function (modelValue, viewValue) {
                    var value = modelValue || viewValue;
                    var numberPattern = "[0-9\\,]+";
                    if (modelValue === undefined || modelValue === null)
                        return true;
                    if (viewValue === undefined || viewValue === null)
                        return true;
                    if (!isNaN(attrs.min) && parseFloat(attrs.min) <= 0) {
                        if (value == "-") return true;
                        numberPattern = "\\-?" + numberPattern;
                    }
                    var valNotZero = false;
                    if (attrs.valueNotZero != undefined) {
                        valNotZero = attrs.valueNotZero;
                        if (valNotZero === "true") {
                            valNotZero = true;
                        } else if (valNotZero === "false") {
                            valNotZero = false;
                        }
                    }
                    if (attrs.isDecimal != undefined) numberPattern += "(\\.[0-9]{0,2})?";
                    numberPattern = "^" + numberPattern + "$";
                    var maxVal = attrs.max == "" ? 0 : parseFloat(attrs.max);
                    return !isNaN(value)
                        && (new RegExp(numberPattern).test(value))
                        && parseFloat(value) >= parseFloat(attrs.min)
                        && parseFloat(value) <= parseFloat(maxVal)
                        && (!valNotZero || parseFloat(value) != 0);
                }
            }
        }
    }])
    .directive('threeDecimalFormat', ['$filter', '$timeout', function ($filter, $timeout) {
        return {
            require: "?ngModel",
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) {
                    return;
                }
                function minVal() {
                    return parseFloat(attrs.min);
                }
                function maxVal() {
                    return parseFloat(attrs.max);
                }
                function getMaxlen() {
                    var res = parseInt(attrs.maxlengthOrigin);
                    function commaCount(val) {
                        var count = 0;
                        var absVal = Math.abs(val);
                        while ((absVal /= 1000) >= 1) count++;
                        return count + (val < 0 ? 1 : 0);
                    }
                    var commaCountMin = commaCount(minVal());
                    var commaCountMax = commaCount(maxVal());
                    res += commaCountMin > commaCountMax ? commaCountMin : commaCountMax;
                    return res;
                }
                attrs.$set("maxlengthOrigin", attrs.maxlength);
                var maxlen = getMaxlen();
                var currMaxVal = minVal();
                var currMinVal = maxVal();
                attrs.$set("maxlength", maxlen);
                attrs.$observe("max", function (newMaxVal) {
                    if (currMaxVal != newMaxVal) {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen);
                        ctrl.$validate();
                        currMaxVal = newMaxVal;
                    }
                });
                attrs.$observe("min", function (newMinVal) {
                    if (currMinVal != newMinVal) {
                        ctrl.$validate();
                        currMinVal = newMinVal;
                    }
                });
                var selectionStart = 0;
                var newNumber = true;
                var dotPressed = false;
                var dashPressed = false;
                var oldCommaCount = 0;
                elem.bind("keydown keypress", function (event) {
                    // determine if keypress will generate new char in model
                    newNumber = (event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105) || event.which == 190 || event.which == 110;
                    if ((event.which == 8 || event.which == 46) && elem[0].selectionStart != elem[0].selectionEnd) {
                        // range deletion
                        // memorize the selection start and let the default behaviour through.
                        selectionStart = elem[0].selectionStart;
                        return;
                    }
                    //minus sign
                    if (event.which == 189 || event.which == 109) {
                        if (elem[0].selectionStart == 0) {
                            // in IE, pressing 189/109 release both 189/109 AND 45 (insert) sequentially
                            // raise this flag to handle the case when insert is not intended
                            dashPressed = true;
                        }
                        else {
                            event.preventDefault();
                            return;
                        }
                    }
                    // handle the dash key
                    if (dashPressed && event.which == 45) {
                        dashPressed = false;
                        newNumber = true;
                    }
                    if ((event.which == 8 && elem[0].selectionStart == elem[0].selectionEnd && elem[0].selectionStart < elem.val().length && elem.val()[elem[0].selectionStart - 1] == ".") ||
                        (event.which == 46 && elem[0].selectionStart == elem[0].selectionEnd && elem[0].selectionStart < elem.val().length - 1 && elem.val()[elem[0].selectionStart] == ".")) {
                        // do not allow deletion of the period "." if there is a decimal part.
                        // to delete the period, delete the decimal part first
                        event.preventDefault();
                        return;
                    }
                    if (event.which == 8 && elem[0].selectionStart > 0) {
                        // take over the default behaviour and emulate backspace key, taking into account the "," char
                        event.preventDefault();
                        // if the char to be deletedis "," - delete the char before it instead
                        var selectionMove = (elem.val()[elem[0].selectionStart - 1] == "," ? 1 : 0);
                        selectionStart = elem[0].selectionStart - 1;
                        var eVal = elem.val();
                        elem.val(eVal.slice(0, selectionStart - selectionMove) + eVal.slice(selectionStart + 1 - selectionMove));
                    }
                    else if (event.which == 46 && elem[0].selectionEnd <= elem.val().length) {
                        // emulate delete key
                        if (dotPressed) {
                            // typing "." will also trigger a delete key. In that case, we let the default behaviour
                            dotPressed = false;
                        } else {
                            // else, take over the default behaviour and emulate delete key taking into account the "," char
                            event.preventDefault();
                            // if the char to be deleted is "," - delete the char after it instead
                            var selectionMove = (elem.val()[elem[0].selectionStart] == "," ? 1 : 0);
                            selectionStart = elem[0].selectionStart;
                            var eVal = elem.val();
                            elem.val(eVal.slice(0, selectionStart + selectionMove) + eVal.slice(selectionStart + 1 + selectionMove));
                        }
                    }
                    else {
                        selectionStart = elem[0].selectionStart;
                    }
                    // no . if there is already a . or cursor leave more than 2 decimal digits
                    if (attrs.isDecimal != undefined && (event.which == 190 || event.which == 110) && (elem.val().indexOf(".") > 0 || elem[0].selectionStart < elem.val().length - 3)) {
                        newNumber = false;
                        event.preventDefault();
                    }
                    if ((attrs.isDecimal == undefined && (event.which == 190 || event.which == 110)) || // no . if decimal is not allowed
                        //(event.which >= 32 && event.which <= 47 && event.which != 46 && event.which != 45) ||
                        (event.which >= 58 && event.which <= 95) ||
                        (event.which >= 106 && event.which <= 107) ||
                        ((minVal() >= 0 || elem[0].selectionStart != 0) && event.which == 109) ||
                        ((minVal() >= 0 || elem[0].selectionStart != 0) && event.which == 189) ||
                        event.which == 32 ||
                        event.which == 111 ||
                        (event.which >= 124 && event.which != 190 && event.which != 189)) {
                        event.preventDefault();
                        newNumber = false;
                    } else if (attrs.isDecimal != undefined) {
                        var dot = elem.val().indexOf(".");
                        if (dot > 0 && dot == elem.val().length - 4 // there is a dot and there is 2 number following the dot
                            && dot < elem[0].selectionStart // the cursor is placed after the dot
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        }
                        if (dot > 0 && dot >= attrs.maxlength - 4  // there is a dot and it is at the maximum pos
                            && dot > elem[0].selectionStart // the cursor is placed before the dot
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        } else if (dot < 0 && elem.val().length >= attrs.maxlength - 4 // there is no dot, and user reached maximum char count for the integer part
                            && elem[0].selectionStart == elem[0].selectionEnd // user not selecting text
                            && ((event.which >= 48 && event.which <= 57) || (event.which >= 96 && event.which <= 105))) { // key pressed is number
                            event.preventDefault();
                            newNumber = false;
                        }
                    }
                    // count the number of comma in old string
                    oldCommaCount = (elem.val().substring(0, selectionStart).match(/,/g) || []).length;
                    if (newNumber) {
                        selectionStart++;
                    }
                    //in IE, pressing . release 190/110 AND 46 sequentially
                    if (event.which == 190 || event.which == 110) {
                        dotPressed = true;
                    }
                });
                elem.bind("focus", function (e) {
                    if ((elem.val() == "0" || elem.val() == "0.000") && attrs.readonly == undefined) {
                        scope.$eval(attrs.ngModel + "=''");
                        elem.val("");
                    }
                    elem.select();
                });
                elem.bind("blur", function (e) {
                    if (elem.val() == "" || elem.val() == "." || elem.val() == "-" || isNaN(ctrl.$modelValue)) {
                        var defaultVal = (attrs.isDecimal == undefined ? "0" : "0.000");
                        scope.$eval(attrs.ngModel + "='" + defaultVal + "'");
                        $timeout(function () {
                            elem.val(defaultVal);
                        });
                    } else if (attrs.isDecimal != undefined) {
                        var val = $filter("number")(ctrl.$modelValue, 3);
                        scope.$eval(attrs.ngModel + "='" + val.replace(/,/g, "") + "'");
                        $timeout(function () {
                            elem.val(val);
                        });
                    }
                    ctrl.$validate();
                });
                // Bind to the mouseup event of the input textbox. 
                elem.bind('mouseup', function () {
                    // Get the old value (before click) and return if it's already empty
                    // as there's nothing to do.
                    var $input = $(this), oldValue = $input.val();
                    if (oldValue === '') return;
                    // Check new value after click, and if it's now empty it means the
                    // clear button was clicked. Manually trigger element's change() event.
                    $timeout(function () {
                        var newValue = $input.val();
                        if (newValue === '') {
                            // return default value
                            var defaultVal = (attrs.isDecimal == undefined ? "0" : "0.000");
                            scope.$eval(attrs.ngModel + "='" + defaultVal + "'");
                            elem.val(defaultVal);
                            angular.element($input).change();
                            ctrl.$validate();
                        }
                    }, 1);
                });
                ctrl.$formatters.unshift(function () {
                    return $filter("number")(ctrl.$modelValue, attrs.isDecimal != undefined ? 3 : 0);
                });
                ctrl.$parsers.unshift(function (viewValue) {
                    var removeCharPattern = "[^0-9"
                        + (attrs.isDecimal != undefined ? "\\." : "")
                        + (parseFloat(attrs.min) < 0 ? "\\-" : "")
                        + "]";
                    var plainNumber = viewValue.replace(new RegExp(removeCharPattern, "g"), "");
                    var formattedNumber;
                    if (plainNumber != "" && plainNumber != "-" && !isNaN(plainNumber)) {
                        var suffix = "";
                        if (plainNumber[plainNumber.length - 1] == "0") {
                            if (plainNumber[plainNumber.length - 2] == ".") {
                                suffix = ".0";
                            } else if (plainNumber[plainNumber.length - 3] == "." && plainNumber[plainNumber.length - 2] == "0") {
                                suffix = ".00";
                            } else if (plainNumber[plainNumber.length - 3] == ".") {
                                suffix = "0";
                            }
                        }
                        formattedNumber = $filter("number")(plainNumber) + suffix;
                    }
                    else
                        formattedNumber = plainNumber;
                    if (plainNumber[plainNumber.length - 1] == '.') formattedNumber += ".";
                    elem.val(formattedNumber);
                    if (minVal() < 0 && formattedNumber[0] != '-') {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen - 1);
                    } else {
                        maxlen = getMaxlen();
                        attrs.$set("maxlength", maxlen);
                    }
                    // fix for ctFormC_IAUtilNorm
                    if (attrs.id == "ctFormC_IAUtilNorm") {
                        attrs.$set("maxlength", 14);
                    }
                    ctrl.$touched = true;
                    var newCommaCount = (formattedNumber.substring(0, selectionStart).match(/,/g) || []).length;
                    if (oldCommaCount < newCommaCount) {
                        selectionStart++;
                    } else if (oldCommaCount > newCommaCount) {
                        selectionStart--;
                    }
                    elem[0].selectionStart = selectionStart;
                    elem[0].selectionEnd = selectionStart;
                    return plainNumber;
                });
                ctrl.$validators.invalid = function (modelValue, viewValue) {
                    var value = modelValue || viewValue;
                    var numberPattern = "[0-9\\,]+";
                    if (modelValue === undefined || modelValue === null)
                        return true;
                    if (viewValue === undefined || viewValue === null)
                        return true;
                    if (!isNaN(attrs.min) && parseFloat(attrs.min) <= 0) {
                        if (value == "-") return true;
                        numberPattern = "\\-?" + numberPattern;
                    }
                    var valNotZero = false;
                    if (attrs.valueNotZero != undefined) {
                        valNotZero = attrs.valueNotZero;
                        if (valNotZero === "true") {
                            valNotZero = true;
                        } else if (valNotZero === "false") {
                            valNotZero = false;
                        }
                    }
                    if (attrs.isDecimal != undefined) numberPattern += "(\\.[0-9]{0,3})?";
                    numberPattern = "^" + numberPattern + "$";
                    var maxVal = attrs.max == "" ? 0 : parseFloat(attrs.max);
                    return !isNaN(value)
                        && (new RegExp(numberPattern).test(value))
                        && parseFloat(value) >= parseFloat(attrs.min)
                        && parseFloat(value) <= parseFloat(maxVal)
                        && (!valNotZero || parseFloat(value) != 0);
                }
            }
        }
    }])
    .directive('capitalize', function () {
        return {
            require: 'ngModel',
            link: function (scope, element, attrs, modelCtrl) {
                var capitalize = function (inputValue) {
                    if (inputValue == undefined) inputValue = '';
                    var capitalized = inputValue.toUpperCase();
                    if (capitalized !== inputValue) {
                        // see where the cursor is before the update so that we can set it back
                        var selection = element[0].selectionStart;
                        modelCtrl.$setViewValue(capitalized);
                        modelCtrl.$render();
                        // set back the cursor after rendering
                        element[0].selectionStart = selection;
                        element[0].selectionEnd = selection;
                    }
                    return capitalized;
                }
                modelCtrl.$parsers.push(capitalize);
                capitalize(scope[attrs.ngModel]); // capitalize initial value
            }
        }
    })
    //.directive('yearValidationCheck', function () {
    //    return {
    //        require: 'ngModel',
    //        link: function (scope, elm, attrs, ngModel) {
    //            scope.$watch(attrs.ngModel, function (yearOfAssessment) {
    //                // set default
    //                ngModel.$setValidity('required', true);
    //                ngModel.$setValidity('yearinvalid', true);
    //                // get value of request type
    //                var elementLetterTypeId = document.querySelector('[id="StatementOfWork_LetterTypeId"]');
    //                var ngModelLetterTypeId = angular.element(elementLetterTypeId).controller('ngModel');
    //                var currentLetterTypeId = ngModelLetterTypeId.$modelValue;
    //                // check exist yearOfAssessment
    //                if (parseInt(currentLetterTypeId) === TYPE_R06_54A ||
    //                    parseInt(currentLetterTypeId) === TYPE_R06_54B ||
    //                    parseInt(currentLetterTypeId) === TYPE_R06_54D ||
    //                    parseInt(currentLetterTypeId) === TYPE_R06_54E) {
    //                    if (yearOfAssessment == undefined || yearOfAssessment === '') {
    //                        ngModel.$setValidity('required', false);
    //                    } else if (yearOfAssessment.length < 4 || yearOfAssessment.length > 40) {
    //                        ngModel.$setValidity('yearinvalid', false);
    //                    } else {
    //                        // get array year items
    //                        var yearArr = yearOfAssessment.split(";");
    //                        // check valid for all items
    //                        angular.forEach(yearArr, function (value, key) {
    //                            value = value.trim();
    //                            if (value == undefined || value === '' || PATTERN_YYYY.test(value) === false ||
    //                                parseInt(value) < YYYY_MIN || parseInt(value) > YYYY_MAX) {
    //                                ngModel.$setValidity('yearinvalid', false);
    //                            }
    //                        });
    //                    }
    //                }
    //            });
    //        }
    //    };
    //})
    .directive('validationMessage', function () {
        return {
            restrict: 'A',
            priority: 1000,
            require: '^validationTooltip',
            link: function (scope, element, attr, ctrl) {
                ctrl.$addExpression(attr.ngIf || true);
            }
        }
    })
    .directive('validationTooltip', function ($timeout) {
        return {
            restrict: 'E',
            transclude: true,
            require: '^form',
            scope: {},
            template: '<span class="label label-danger span1" ng-show="errorCount > 0"><i class="fa fa-exclamation-triangle" aria-hidden="true"></i></span>',
            controller: function ($scope) {
                var expressions = [];
                $scope.errorCount = 0;
                this.$addExpression = function (expr) {
                    expressions.push(expr);
                };
                $scope.$watch(function () {
                    var count = 0;
                    angular.forEach(expressions, function (expr) {
                        if ($scope.$eval(expr)) {
                            ++count;
                        }
                    });
                    return count;
                }, function (newVal) {
                    $scope.errorCount = newVal;
                });
            },
            link: function (scope, element, attr, formController, transcludeFn) {
                scope.$form = formController;
                transcludeFn(scope, function (clone) {
                    var badge = element.find('.label');
                    var tooltip = angular.element('< div class= "validationMessageTemplate tooltip-danger" />');
                    tooltip.append(clone);
                    element.append(tooltip);
                    $timeout(function () {
                        scope.$field = formController[attr.target];
                        badge.tooltip({
                            placement: 'right',
                            html: true,
                            title: clone
                        });
                    });
                });
            }
        };
    })
    .directive('noTextDateTime', function ($filter) {
        return {
            require: '?ngModel',
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) {
                    return;
                }
                elem.bind("keydown keypress", function (event) {
                    if ((event.which === 190 || event.which === 110) ||
                        (event.which >= 58 && event.which <= 95) ||
                        (event.which >= 106 && event.which <= 107) ||
                        event.which === 32 ||
                        event.which === 173 || event.which === 109 ||
                        (event.which >= 124 && event.which !== 191)) {
                        event.preventDefault();
                    }
                });
            }
        }
    })
    .directive('blockerPaste', function ($filter) {
        return {
            require: '?ngModel',
            link: function (scope, elem, attrs, ctrl) {
                if (!ctrl) {
                    return;
                }
                elem.bind("keydown keypress", function (event) {
                    if (event.which === 17 || event.which === 86) {
                        event.preventDefault();
                    }
                });
            }
        }
    })
    //.directive('checkDateInvalid', function () {
    //    return {
    //        require: 'ngModel',
    //        link: function (scope, elm, attrs, ngModel) {
    //            scope.$watch(attrs.ngModel, function (value) {
    //                // set default
    //                ngModel.$setValidity('invalid', true);
    //                ngModel.$setValidity('datelessthancurrent', true);
    //                // check
    //                if (value != undefined && value !== '') {
    //                    if (value.length !== 10) {
    //                        ngModel.$setValidity('invalid', false);
    //                    } else {
    //                        var valueArr = value.split("/");
    //                        // check only numbers
    //                        var reg = new RegExp('^\\d*$');
    //                        if (valueArr.length === 3 &&
    //                            valueArr[0] != undefined && valueArr[0] !== '' && valueArr[0].length === 2 && valueArr[0].match(reg) &&
    //                            valueArr[1] != undefined && valueArr[1] !== '' && valueArr[1].length === 2 && valueArr[1].match(reg) &&
    //                            valueArr[2] != undefined && valueArr[2] !== '' && valueArr[2].length === 4 && valueArr[2].match(reg)) { // check valid date
    //                            // check valid date selected
    //                            value = valueArr[2] + ' ' + valueArr[1] + ' ' + valueArr[0];
    //                            var isValid = moment(value, "YYYY MM DD").isValid();
    //                            ngModel.$setValidity('invalid', isValid);
    //                            // check if DocumentSignDate
    //                            if (isValid && (ngModel.$name === 'DocumentSignDate' || ngModel.$name === 'StatementOfWork_DocumentSignDate')) {
    //                                // check with current date
    //                                var selected = new Date(valueArr[2], (valueArr[1] - 1), valueArr[0]);
    //                                var current = new Date();
    //                                if (selected > current) {
    //                                    ngModel.$setValidity('datelessthancurrent', false);
    //                                } else {
    //                                    ngModel.$setValidity('datelessthancurrent', true);
    //                                }
    //                            }
    //                        } else {
    //                            ngModel.$setValidity('invalid', false);
    //                        }
    //                    }
    //                }
    //            });
    //        }
    //    };
    //})
    .directive('validFile', function () {
        return {
            require: 'ngModel',
            link: function (scope, el, attrs, ngModel) {
                //change event is fired when file is selected
                el.bind('change', function () {
                    scope.$apply(function () {
                        ngModel.$setViewValue(el.val());
                        ngModel.$render();
                    });
                });
            }
        };
    })
    .directive("kNgDisabled", function () {
        return {
            restrict: "A",
            link: function (scope, element, attr) {
                scope.$on("kendoWidgetCreated", function (e, widget) {
                    var value = attr.kNgDisabled === "true";
                    $(widget.body).attr("contenteditable", value);
                    scope.$watch(attr.kNgDisabled, function (value) {
                        $(widget.body).attr("contenteditable", value);
                    });
                });
            }
        };
    })
    .directive("decimals", function ($filter) {
        return {
            restrict: "A", // Only usable as an attribute of another HTML element
            require: "?ngModel",
            scope: {
                decimals: "@",
                decimalPoint: "@"
            },
            link: function (scope, element, attr, ngModel) {
                var decimalCount = parseInt(scope.decimals) || 2;
                var decimalPoint = scope.decimalPoint || ".";

                // Run when the model is first rendered and when the model is changed from code
                ngModel.$render = function () {
                    if (ngModel.$modelValue != null && ngModel.$modelValue >= 0) {
                        if (typeof decimalCount === "number") {
                            element.val(ngModel.$modelValue.toFixed(decimalCount).toString().replace(".", ","));
                        } else {
                            element.val(ngModel.$modelValue.toString().replace(".", ","));
                        }
                    }
                }

                // Run when the view value changes - after each keypress
                // The returned value is then written to the model
                ngModel.$parsers.unshift(function (newValue) {
                    if (typeof decimalCount === "number") {
                        var floatValue = parseFloat(newValue.replace(",", "."));
                        if (decimalCount === 0) {
                            return parseInt(floatValue);
                        }
                        return parseFloat(floatValue.toFixed(decimalCount));
                    }

                    return parseFloat(newValue.replace(",", "."));
                });

                // Formats the displayed value when the input field loses focus
                element.on("change", function (e) {
                    var floatValue = parseFloat(element.val().replace(",", "."));
                    if (!isNaN(floatValue) && typeof decimalCount === "number") {
                        if (decimalCount === 0) {
                            element.val(parseInt(floatValue));
                        } else {
                            var strValue = floatValue.toFixed(decimalCount);
                            element.val(strValue.replace(".", decimalPoint));
                        }
                    }
                });
            }
        }
    })
    .directive('allowDecimalNumbers', function () {
        return {
            restrict: 'A',
            link: function (scope, elm, attrs, ctrl) {
                elm.on('keydown', function (event) {
                    var $input = $(this);
                    var value = $input.val();
                    value = value.replace(/[^0-9\.]/g, '')
                    var findsDot = new RegExp(/\./g)
                    var containsDot = value.match(findsDot)
                    if (containsDot != null && ([46, 110, 190].indexOf(event.which) > -1)) {
                        event.preventDefault();
                        return false;
                    }
                    $input.val(value);
                    if (event.which == 64 || event.which == 16) {
                        // numbers  
                        return false;
                    } if ([8, 13, 27, 37, 38, 39, 40, 110].indexOf(event.which) > -1) {
                        // backspace, enter, escape, arrows  
                        return true;
                    } else if (event.which >= 48 && event.which <= 57) {
                        // numbers  
                        return true;
                    } else if (event.which >= 96 && event.which <= 105) {
                        // numpad number  
                        return true;
                    } else if ([46, 110, 190].indexOf(event.which) > -1) {
                        // dot and numpad dot  
                        return true;
                    } else {
                        event.preventDefault();
                        return false;
                    }
                });
            }
        }
    }); 