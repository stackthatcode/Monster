
var Monster = Monster || {};

Monster.IsDate = function(input) {
    var date = new Date(input);
    return (date instanceof Date && !isNaN(date.valueOf()));
};

Monster.FindByField = function(list, fieldName, fieldValue) {
    return AQ(list)
        .firstOrDefault(function (x) { return x[fieldName] == fieldValue; });
}
