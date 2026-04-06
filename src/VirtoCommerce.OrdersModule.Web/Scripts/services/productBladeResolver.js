angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.productBladeResolver', [
        '$q',
        function ($q) {
            var handlers = [];

            function sortHandlers() {
                handlers = _.sortBy(handlers, function (x) {
                    return x.priority || 100;
                });
            }

            function registerHandler(handler, priority) {
                handlers.push({
                    fn: handler,
                    priority: angular.isNumber(priority) ? priority : 100
                });

                sortHandlers();
            }

            function open(context) {
                var index = 0;

                function next() {
                    if (index >= handlers.length) {
                        return $q.when(false);
                    }

                    var current = handlers[index++];

                    return $q.when(current.fn(context)).then(function (handled) {
                        if (handled) {
                            return true;
                        }

                        return next();
                    });
                }

                return next();
            }

            return {
                registerHandler: registerHandler,
                open: open
            };
        }
    ]);
