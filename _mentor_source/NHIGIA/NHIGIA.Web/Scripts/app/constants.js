"use strict";

var STRING_EMPTY = "";
var COMMA_EMPTY = ", ";
var DATE_FORMAT_CAPLOCK = "DD/MM/YYYY";
var DATE_FORMAT = "dd/MM/yyyy";
var DATE_FORMAT_MONTH = "MM/yyyy";
var MESSAGE_ERROR_FOR_INPUT = "Yêu cầu nhập dữ liệu";
var MESSAGE_ERROR_FOR_SELECT = "Yêu cầu chọn dữ liệu";
var MESSAGE_ERROR_DUPLICATE = "Tên đã tồn tại";
var MESSAGE_ERROR_DUPLICATE_DATA = "Dữ liệu đã tồn tại";
var MESSAGE_ERROR_FOR_NOTEQUAS = "Mật khẩu mới và mật khẩu nhập lại chưa trùng nhau";
var MESSAGE_ERROR_FOR_FILE = "Yêu cầu chọn file";
var ROW = "row";
var SINGLE = "single";
var CONTAINS = "contains";

// message bootbox
var MSG_DELETED_CONFIRM = "<span style='color:green; text-align:justify;'>Bạn có chắc chắn muốn xóa bản ghi này không?</span>";
var MSG_DELETED_REMOVE = "<span style='color:green; text-align:justify;'>Bạn có chắc chắn muốn loại bỏ đối tượng này không?</span>";
var MSG_INSERT_CONTRACT_CONFIRM = "Hợp đồng hiện tại sẽ được chấm dứt hiệu lực sau khi sửa vì ngày có hiệu lực của hợp đồng này nhỏ hơn ngày có hiệu lực của hợp đồng hiện tại. Bạn có muốn thực hiện thêm hợp đồng mới không?";
var MSG_CHANGE_CONFIRM = "<span style='color:green; text-align:justify;'>Bạn có chắc chắn muốn thay đổi trạng thái của bản ghi này không?</span>";

var MSG_RESETPASSWORD_CONFIRM = "<span style='color:green; text-align:justify;'>Bạn có chắc chắn muốn đặt lại mật khẩu cho tài khoản này không?</span>";
var MSG_RESETPASSWORD_SUCCESS = "<span style='color:green; text-align:justify;'>Mật khẩu được đặt lại thành công</span>";
var CANNOT_FIND_ANY_WITH_GIVEN_ID = "Không thể tìm thấy bất kỳ với Id đã cho";

var NORECORDS = "Không có dữ liệu. Vui lòng thêm mới";
var CONTENT_FILTER = "Nội dung lọc";
var FILTER = "Lọc";
var CLEAR = "Xóa";
var ITEMSPERPAGE = "mục trên mỗi trang";
var DISPLAY = "{0} - {1} của {2} mục";
var EMPTY = "Không có mục nào";
var SELECTEDITEMSFORMAT = "{0} mục";

//ListCategoryTypeEnum
var WorkUnit = 1;
var JobPosition = 2;
var Sex = 3;
var Nation = 4;
var Religion = 5;
var EducationalLevel = 6;
var DegreeTraining = 7;
var TrainingPlaces = 8;
var Faculty = 9;
var Specialized = 10;
var Classification = 11;
var MaritalStatus = 12;
var FamilyMember = 13;
var IngredientsThemselves = 14;
var Relationship = 15;
var WorkLocation = 16;
var ContractType = 17;
var Wage = 18;
var StandardPublic = 19;
var Bank = 20;
var PlaceOfRegistrationForMedicalExaminationAndTreatment = 21;
var GroupPosition = 22;
var PartyPosition = 23;
var BloodGroup = 24;
var Army = 25;
var MilitaryRank = 26;
var MilitaryPosition = 27;
var Rank = 28;
var ContractTerm = 29;
var TheFormOfWork = 30;
var RewardReason = 31;
var CommendationForm = 32;
var StatusBonus = 33;
var TypeOfIncident = 34;
var CompensationStatus = 35;
var FormsOfTraining = 36;
var CertificateGroup = 37;
var SkillGroup = 38;
var SkillLevel = 39;
var ProcedureGroupReceive = 40;
var ProcedureGroupQuit = 41;
var AssetType = 42;
var AssetStatus = 43;
var FontSize = 44;
var NumberSize = 45;
var EvaluationPeriod = 46;
var EvaluationStatus = 47;
var AllowanceType = 48;
var RewardPlan = 49;
var BonusBudgetSource = 50;
var InjuryCondition = 51;
var KindOfDecision = 52;
var FormsProcessing = 53;

var WorkStatus = "WorkStatus";
var BrowsingStatus = "BrowsingStatus";
var NameOfTheDebt = "NameOfTheDebt";

function introduceDot(number) {
    var numberString = number.toString();
    var length = numberString.length;
    if (length < 4)
        return numberString;

    var insertIndex = length - 3;  // 10.000.000    8
    var loopCounter = length / 3;
    for (var i = 0; i < loopCounter; i++) {
        if (insertIndex < 1)
            break;
        numberString = numberString.insert(insertIndex, '.');
        insertIndex = insertIndex - 3;
    }
    return numberString;
}

function removeDot(number) {
    var numberString = number.toString();
    numberString = numberString.replaceAll('.', '');
    return parseInt(numberString);
}

function introduceComma(number) {
    var numberString = number.toString();
    var length = numberString.length;
    if (length < 4)
        return numberString;

    var insertIndex = length - 3;  // 10,000,000    8
    var loopCounter = length / 3;
    for (var i = 0; i < loopCounter; i++) {
        if (insertIndex < 1)
            break;
        numberString = numberString.insert(insertIndex, ',');
        insertIndex = insertIndex - 3;
    }
    return numberString;
}

function removeComma(number) {
    var numberString = number.toString();
    numberString = numberString.replaceAll(',', '');
    return parseInt(numberString);
}

String.prototype.insert = function (index, string) {
    if (index > 0)
        return this.substring(0, index) + string + this.substring(index, this.length);
    else
        return string + this;
};

String.prototype.replaceAll = function (search, replacement) {
    var target = this;
    return target.replace(new RegExp(search, 'g'), replacement);
};