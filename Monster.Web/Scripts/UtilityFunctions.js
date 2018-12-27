
var Monster = Monster || {};

Monster.IsDate = function(input) {
    var date = new Date(input);
    return (date instanceof Date && !isNaN(date.valueOf()));
};
