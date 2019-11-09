
var Monster = Monster || {};

Monster.IsDate = function(input) {
    var date = new Date(input);
    return (date instanceof Date && !isNaN(date.valueOf()));
};

Monster.FindByField = function(list, fieldName, fieldValue) {
    return AQ(list)
        .firstOrDefault(function (x) { return x[fieldName] == fieldValue; });
}

Monster.NumberFormat = function(input) {
    return input == null || isNaN(input) ? "N/A" : input.toFixed(2);
};


// Gratitude to David Walsh - https://davidwalsh.name/pubsub-javascript
//
Monster.Events = (function () {
    var topics = {};
    var hOP = topics.hasOwnProperty;

    return {
        subscribe: function (topic, listener) {
            // Create the topic's object if not yet created
            if (!hOP.call(topics, topic)) topics[topic] = [];

            // Add the listener to queue
            var index = topics[topic].push(listener) - 1;

            // Provide handle back for removal of topic
            return {
                remove: function () {
                    delete topics[topic][index];
                }
            };
        },
        publish: function (topic, info) {
            // If the topic doesn't exist, or there's no listeners in queue, just leave
            if (!hOP.call(topics, topic)) return;

            // Cycle through topics queue, fire!
            topics[topic].forEach(function (item) {
                item(info != undefined ? info : {});
            });
        }
    };
})();
