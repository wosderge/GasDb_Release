// GASでjsonよみかきするやーつ v12/16

// これ定義しないとなんかデプロイ時にエラーでるっぽいので
function doGet(e) {
    return "";
}

// ここが事実上の入り口
function doPost(e) {
    var request = e.parameter;
    var method = request["method"];

    if (method == "GetLog") {
        return ContentService.createTextOutput(JSON.stringify(getlog(e)));
    }
    if (method == "GetData") {
        return ContentService.createTextOutput(JSON.stringify(getjson(e)));
    }
    if (method == "SetData") {
        return saveJson(e);
    }
    if (method == "UpdateData") {
        return UpdateData(e);
    }

    return ContentService.createTextOutput("Error: Invalid method.");
}


// シート要求
function DemandSheet(ss, sheetname) {

    const sheet = ss.getSheetByName(sheetname);
    if (sheet) {
        Logger.log("あるね");
        return sheet;
    } else {
        Logger.log("ないからつくるね");
        let newSheet = ss.insertSheet();
        newSheet.setName(sheetname);
        var content = [];
        content.push(["objectId", "createDate", "updateDate"]);
        content.push(["string", "DateTime", "DateTime"]);
        newSheet.getRange(1, 1, 2, 3).setValues(content);
        return newSheet;
    }
}


// ヘッド調査
function Headding(sheet, values, json) {
    var hens = [];
    var types = [];
    var h = values[0].length;
    for (i = 0; i < values[0].length; i++) {
        hens.push(values[0][i]);
        types.push(values[1][i]);
    }
    for (js in json) {
        if (js == "sheetName") continue;
        if (js == "acl") continue;
        if (values[0].includes(js)) {
        } else {
            Logger.log("含まないので追加");
            sheet.getRange(1, h + 1).setValue(js);
            var typer = CheckType(json[js]);
            sheet.getRange(2, h + 1).setValue(typer);
            hens.push(js);
            types.push(typer);
            h = h + 1;
        }

    }
    var header = [];
    header.push(hens);
    header.push(types);
    return header;
}

// 型調査
function CheckType(kore) {
    switch (typeof (kore)) {
        case "number":
            if (Number.isInteger(kore)) {
                return "int";
            } else {
                return "float";
            }
            break;
        case "boolean":
            return "bool";
            break;
        case "string":
            return "string";
            break;
        case "object":
            return "object";
            break;
        default:
            return tyypeof(kore);
            break;
    }
}


// 時間生成
function GetTime() {
    return Utilities.formatDate(new Date(), "GMT+9", "yyyy/MM/dd HH:mm:ss");
}

// ランダムid生成
function RandomId() {
    var results = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var res = "";
    for (var i = 0; i < 16; i++) {
        res += results[Math.floor(Math.random() * 62)];
    }
    return res;
}

//　てきとーにID生成しまくる
function RandomIDMaker() {
    // スプレッドシート・シート取得
    const ss = SpreadsheetApp.getActiveSpreadsheet();
    const sheet = ss.getSheetByName("randama");
    var contentarray = [];
    for (var j = 0; j < 20; j++) {
        var content = [];
        content[0] = RandomId();
        contentarray.push(content);
    }
    sheet.getRange(1, 1, 20, 1).setValues(contentarray);
}


function CheckQuery(r, queries) {
    var en = true;
    for (qi in queries) {
        var q = queries[qi];

        var value = q["value"];
        var rr = r[q["key"]];

        switch (q["condition"]) {
            case "in":
                if (!(rr.match(value))) en = false;
                break;
            case "nin":
                if ((rr.match(value))) en = false;
                break;
            case "anyeq":
                {
                    for (vv in q["any"]) {
                        if (rr == q["any"][vv]) return true;
                    }
                    return false;
                }
                break;
            case "eq":
                if (!(rr == value)) en = false;
                break;
            case "ne":
                if (!(rr != value)) en = false;
                break;
            case "lt":
                if (!(rr < value)) en = false;
                break;
            case "lte":
                if (!(rr <= value)) en = false;
                break;
            case "gt":
                if (!(rr > value)) en = false;
                break;
            case "gte":
                if (!(rr >= value)) en = false;
                break;
        }
    }

    return en;

}


// データの更新
function UpdateData(e) {

    // 引数を分解
    var request = e.parameter;
    var sheetName = request["sheetName"];
    let mode = request["mode"];
    var query = request["query"];
    var value = request["value"];
    var key = request["key"];
    var queries;

    var log = "";
    const ss = SpreadsheetApp.getActiveSpreadsheet();
    var sheet = DemandSheet(ss, sheetName);
    var range = sheet.getDataRange();
    var values = range.getDisplayValues();

    if (query) {
        var qbase = JSON.parse(query);
        var queries = qbase["data"];
    }
    var koko = -1;
    for (i = 0; i < values[0].length; i++) {
        log += i + ":" + values[0][i] + "==" + key + "\n";
        if (values[0][i] == key) { koko = i; break; }
    }
    if (koko == -1) return ContentService.createTextOutput("no key : " + log);

    for (var j = 2; j < values.length; j++) {
        if (queries) {
            var r = GetRow(values[j], values, sheetName);
            if (CheckQuery(r, queries) == false) continue;
        }
        if (mode == "add") {
            switch (values[1][koko]) {
                case "int":
                    sheet.getRange(j + 1, koko + 1).setValue(Number.parseInt(values[j][koko]) + Number.parseInt(value));
                    break;
                case "float":
                    sheet.getRange(j + 1, koko + 1).setValue(Number.parseFloat(values[j][koko]) + Number.parseFloat(value));
                    break;
            }
        } else {
            sheet.getRange(j + 1, koko + 1).setValue(value);

        }
    }
    return ContentService.createTextOutput(log);
}


// json　保存
function saveJson(e) {

    // 引数を分解
    var request = e.parameter;
    var sheetName = request["sheetName"];
    var isArray = request["isArray"];
    let json = JSON.parse(request["json"]);
    let mode = request["mode"];


    const ss = SpreadsheetApp.getActiveSpreadsheet();
    var sheet = DemandSheet(ss, sheetName);
    var range = sheet.getDataRange();
    var values = range.getDisplayValues();

    var header
    var keyid = json["objectId"];

    if (isArray == "ARRAY") { // NCMBからの移植用
        var log = "";
        Logger.log("jsonview.");
        let json2 = json["results"];
        header = Headding(sheet, values, json2[0]);
        log += "  json:" + json2.length;
        log += "  header:" + header[0].length;
        var contentarray = [];

        for (var j = 0; j < json2.length; j++) {
            var content = [];
            var jsonr = json2[j];
            content[0] = jsonr["objectId"];
            content[1] = jsonr["createDate"]["iso"];
            content[2] = jsonr["updateDate"]["iso"];
            for (var i = 3; i < header[0].length; i++) {

                content[i] = jsonr[header[0][i]];

            }
            //最終行にデータを追加
            contentarray.push(content);
        }
        var width = header[0].length;
        var height = contentarray.length;
        sheet.getRange(3, 1, height, width).setValues(contentarray);
        sheet.getRange(2, 1, 1, header[0].length).setBackground('#fff2cc');
        sheet.getRange(1, 1, 1, header[0].length).setBackground('#ccf2cc');
        return ContentService.createTextOutput(log);

    } else if (keyid == null || keyid == "") { // objectIdがない場合は新規レコードを追加
        Logger.log("new line");

        header = Headding(sheet, values, json);

        var content = [];
        json["sheetName"] = sheetName;;
        json["objectId"] = content[0] = RandomId();
        json["createDate"] = content[1] = GetTime();
        json["updateDate"] = content[2] = GetTime();
        for (var i = 3; i < header[0].length; i++) {

            if (header[1][i] == "object") {
                content[i] = JSON.stringify(json[header[0][i]]);
            } else {
                content[i] = json[header[0][i]];
            }

        }
        //最終行にデータを追加
        sheet.appendRow(content);

    } else { //ある場合は探して更新
        Logger.log("find line");
        header = Headding(sheet, values, json);

        for (var j = 2; j < values.length; j++) {
            if (values[j][0] != keyid) continue;
            json["updateDate"] = GetTime();
            sheet.getRange(j + 1, 3).setValue(GetTime());
            for (var i = 3; i < header[0].length; i++) {
                if (header[1][i] == "object") {
                    sheet.getRange(j + 1, i + 1).setValue(JSON.stringify(json[header[0][i]]));
                } else {
                    sheet.getRange(j + 1, i + 1).setValue(json[header[0][i]]);
                }

            }
        }

    }
    sheet.getRange(2, 1, 1, header[0].length).setBackground('#fff2cc');
    sheet.getRange(1, 1, 1, header[0].length).setBackground('#ccf2cc');
    return ContentService.createTextOutput(JSON.stringify(json));
}

// jsonを取得する
function getjson(e) {

    var request = e.parameter;
    var sheetName = request["sheetName"];
    var key = request["key"];
    var query = request["query"];
    var order = request["order"];
    // スプレッドシート・シート取得
    const ss = SpreadsheetApp.getActiveSpreadsheet();
    const sheet = ss.getSheetByName(sheetName);

    // 値が入力されている範囲を取得
    const range = sheet.getDataRange();
    // 画面表示されている通りに値を2次元配列に入れる
    const values = range.getDisplayValues();
    if (!key) { // 配列で全取得
        let jsonObj = [];
        // 2行目(データ行)からループ 1行目しかない(データがない)場合ループに入らない
        var queries;
        if (query) {
            var qbase = JSON.parse(query);
            var queries = qbase["data"];
        }
        for (const row of values.slice(2)) {
            var r = GetRow(row, values, sheetName);
            //クエリでフィルタリング
            if (queries) {
                if (CheckQuery(r, queries) == false) continue;
            }

            jsonObj.push(r);
        }


        //　ソートする  
        if (order) {
            var orderjson = JSON.parse(order);
            var isDesc = orderjson.anti ? -1 : 1;
            var target = orderjson.key;
            jsonObj = jsonObj.sort(function (a, b) {
                if (a[target] > b[target]) return 1 * isDesc;
                if (a[target] < b[target]) return -1 * isDesc;
                return 0;
            });
        }
        return jsonObj;

    } else { // object id で探査
        for (const row of values.slice(2)) {
            if (row[0] != key) continue;
            return GetRow(row, values, sheetName);
        }
    }
}

// rowの情報を取得して返す
function GetRow(row, values, sheetName) {
    const tempObj = {};
    tempObj["sheetName"] = sheetName;
    for (const index in row) {
        switch (values[1][index]) {
            case "object":
                tempObj[values[0][index]] = JSON.parse(row[index]);
                break;
            case "bool":
                tempObj[values[0][index]] = (row[index] == "TRUE");
                break;
            case "int":
                tempObj[values[0][index]] = Number.parseInt(row[index]);
                break;
            case "float":
                tempObj[values[0][index]] = Number.parseFloat(row[index]);
                break;
            default:
                tempObj[values[0][index]] = row[index];
                break;
        }
    }
    return tempObj;
}


// ログ取得

function getlog(e) {

    var request = e.parameter;
    var sheetName = request["sheetName"];
    var limit = request["limit"];
    var loadStartLineNum = request["loadStartLineNum"]
    // スプレッドシート・シート取得
    const ss = SpreadsheetApp.getActiveSpreadsheet();
    const sheet = DemandSheet(ss, sheetName);

    // 値が入力されている範囲を取得
    var range = sheet.getDataRange();
    // 画面表示されている通りに値を2次元配列に入れる
    var values = range.getDisplayValues();

    let jsonObj = [];

    if (request["json"]) {
        let json = JSON.parse(request["json"]);
        header = Headding(sheet, values, json);

        var content = [];
        json["sheetName"] = sheetName;;
        json["objectId"] = content[0] = RandomId();
        json["createDate"] = content[1] = GetTime();
        json["updateDate"] = content[2] = GetTime();
        for (var i = 3; i < header[0].length; i++) {

            if (header[1][i] == "object") {
                content[i] = JSON.stringify(json[header[0][i]]);
            } else {
                content[i] = json[header[0][i]];
            }

        }
        //最終行にデータを追加
        sheet.appendRow(content);
        jsonObj.push(json);
    }

    var count = 0;

    for (j = values.length - 1; j > loadStartLineNum; j--) {
        if (count == limit) break;
        var r = {};
        Logger.log(j + " / " + values[j]);
        for (index = 0; index < values[j].length; index++) {
            Logger.log(values[1][index]);
            switch (values[1][index]) {
                case "object":
                    r[values[0][index]] = JSON.parse(values[j][index]);
                    break;
                case "bool":
                    r[values[0][index]] = (values[j][index] == "TRUE");
                    break;
                case "int":
                    r[values[0][index]] = Number.parseInt(values[j][index]);
                    break;
                case "float":
                    r[values[0][index]] = Number.parseFloat(values[j][index]);
                    break;
                default:
                    r[values[0][index]] = values[j][index];
                    break;
            }
        }

        jsonObj.push(r);
        count++;
    }
    Logger.log(jsonObj);

    let jsonMaster = {};
    jsonMaster["line"] = values.length;
    jsonMaster["data"] = jsonObj;
    return jsonMaster;
}