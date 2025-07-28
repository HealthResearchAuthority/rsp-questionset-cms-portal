angular.module("umbraco").controller("CreateNewVersionController", function ($scope, notificationsService, navigationService, treeService, eventsService, $http) {
    $scope.create = function () {
        let nodeParent = $scope.currentNode.parent();
        $scope.loading = true;        

        $http.post("/umbraco/backoffice/api/CreateNewVersionApi/DeepCopyWithRelink", {
            id: $scope.currentNode.id
        }).then(function (response) {
            notificationsService.success("New version created sucesfully.");

            // close dialog
            navigationService.hideMenu();

            // reload tree to show the newly created version
            treeService.loadNodeChildren({ node: nodeParent }); // reload children from server

        }, function (error) {
            notificationsService.error("Error", error.data.message || "Something went wrong");
        }).finally(function () {
            $scope.loading = false;
        });
    };
    $scope.cancel = function () {
        navigationService.hideMenu();
    };
});