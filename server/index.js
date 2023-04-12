class MapEntry {
    constructor(row, subrow) {
        this.MapRow = row;
        this.SubRow = subrow;
        this.Key = GenerateKey();
        this.Created = Date.now();
    }
}

const ErrorType = {
    TooManyContentID: "too many ids requested",
    InvalidContentID: "invalid id",
    DuplicateContentID: "duplicate id",
    InvalidMapID: "invalid map id",
    InvalidKey: "invalid key",
    AlreadyInUse: "already in use",
    InvalidRequest: "invalid request",
}

const express = require("express");

const app = express();

app.listen(40001, "127.0.0.1", () => {
    console.log("Server Started on Port 40001.");
});

const Entries = {

};

const IsValidContentId = (contentId) => /^[0-9A-F]{32}$/g.test(contentId);

const GenerateKey = () => {
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    let counter = 0;
    while (counter < 32) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
      counter += 1;
    }
    return result;
}

const ValidId = (id) => {
    if (!/^[0-9]+$/g.test(id)) return false;
    if (isNaN(id)) return false;
    const i = parseInt(id);
    if (i < 0 || i > 100) return false;
    return i;
}

const ValidMap = (row, subrow) => {
    const ret = {
        Row: ValidId(row),
        SubRow: ValidId(subrow)
    };
    if (ret.Row !== false && ret.SubRow !== false) return ret;
    return false;
};



const Error = (res, code, type) => {
    // console.log(`Send Error: ${type}`);
    res.statusCode = code;
    res.send({error: type ?? true});
}


app.get("/:contentIds", (req, res) => {
    const contentIds = req.params.contentIds.split(',');
    if (contentIds.length < 1 || contentIds.length > 7) return Error(res, 400, ErrorType.TooManyContentID);
    const response = [];
    for (let id of contentIds) {
        if (id in response) return Error(res, 400, ErrorType.DuplicateContentID);
        if (!IsValidContentId(id)) return Error(res, 400, ErrorType.InvalidContentID);
        response.push({ ID: id, Row: Entries[id]?.MapRow ?? 0, SubRow: Entries[id]?.SubRow ?? 0 });
    }
    res.send(response);
});

app.put("/:contentId/:row/:subrow", (req, res) => {
    const contentId = req.params.contentId;
    if (!IsValidContentId(contentId)) return Error(res, 400, ErrorType.InvalidContentID);
    if (contentId in Entries) return Error(res, 400, ErrorType.AlreadyInUse);
    var map = ValidMap(req.params.row, req.params.subrow);
    if (map === false) return Error(res, 400, ErrorType.InvalidMapID);
    Entries[contentId] = new MapEntry(map.Row, map.SubRow);
    console.log(`[CREATE] ${contentId} => ${map.Row}, ${map.SubRow}`);
    res.send(Entries[contentId]);
});

app.patch("/:contentId/:key/:row/:subrow", (req, res) => {
    const contentId = req.params.contentId;
    const key = req.params.key;
    if (!IsValidContentId(contentId)) return Error(res, 400, ErrorType.InvalidContentID);
    var map = ValidMap(req.params.row, req.params.subrow);
    if (map === false) return Error(res, 400, ErrorType.InvalidMapID);
    if (!/^[A-Za-z0-9$]{32}/g.test(key)) return Error(res, 400, ErrorType.InvalidKey);
    if (!(contentId in Entries)) return Error(res, 400, ErrorType.InvalidKey);
    var entry = Entries[contentId];
    if (entry.Key != key) return Error(res, 400, ErrorType.InvalidKey);
    console.log(`[UPDATE] ${contentId} => ${map.Row}, ${map.SubRow}`);
    entry.MapRow = map.Row;
    entry.SubRow = map.SubRow;
    entry.Created = Date.now();

    res.send(entry);
});

app.delete("/:contentId/:key", (req, res) => {
    const contentId = req.params.contentId;
    const key = req.params.key;
    if (!IsValidContentId(contentId)) return Error(res, 400, ErrorType.InvalidContentID);
    if (!/^[A-Za-z0-9$]{32}/g.test(key)) return Error(res, 400, ErrorType.InvalidKey);
    if (!(contentId in Entries)) return Error(res, 400, ErrorType.InvalidKey);
    console.log(`[DELETE] ${contentId}`);
    delete Entries[contentId];
    res.send({error: false});
});

app.use("*", (req, res) => {
    console.log(`Invalid Request: ${req.originalUrl}`);
    return Error(res, 400, ErrorType.InvalidRequest);
});


setInterval(() => {
    var timeNow = Date.now();
    for (var k in Entries) {
        let age = timeNow - Entries[k].Created;
        if (age < 0 || age > 300000) {
            console.log(`[AUTO DELETE] ${k}`);
            delete Entries[k];
        }
    }
}, 1000);