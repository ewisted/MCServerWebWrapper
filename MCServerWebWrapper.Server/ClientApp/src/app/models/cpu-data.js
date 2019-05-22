"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var CpuData = /** @class */ (function () {
    function CpuData() {
        this.cpuLogs = [];
        this.xValues = [];
        for (var i = 1; i <= 60; i++) {
            this.xValues.push(i * 7);
        }
    }
    CpuData.prototype.addData = function (cpuUsage) {
        if (this.cpuLogs.length >= 60) {
            this.cpuLogs.pop();
        }
        this.cpuLogs.unshift((100 - cpuUsage) * 2);
        return this.getString();
    };
    CpuData.prototype.getString = function () {
        var arr = [];
        for (var i = 0; i < this.cpuLogs.length; i++) {
            arr.push(this.xValues[i] + "," + this.cpuLogs[i]);
        }
        return arr.join(" ");
    };
    return CpuData;
}());
exports.CpuData = CpuData;
//# sourceMappingURL=cpu-data.js.map