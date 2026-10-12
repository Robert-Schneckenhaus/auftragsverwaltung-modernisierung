function fehlertext(err) {
    if (err && err.data && err.data.Message) return err.data.Message;
    return 'Unbekannter Fehler';
}

avApp.controller('LoginCtrl', function ($scope, $rootScope, $location, Api) {
    $scope.daten = {};
    $scope.anmelden = function () {
        Api.login($scope.daten.login, $scope.daten.passwort).then(function (res) {
            $rootScope.benutzer = res.data;
            localStorage.setItem('benutzer', JSON.stringify(res.data));
            $location.path('/auftraege');
        }, function (err) {
            $scope.fehler = fehlertext(err);
        });
    };
});

avApp.controller('KundenCtrl', function ($scope, Api) {
    $scope.suche = '';
    $scope.laden = function () {
        Api.kunden($scope.suche).then(function (res) {
            $scope.kunden = res.data;
        });
    };
    $scope.laden();
});

avApp.controller('KundeCtrl', function ($scope, $routeParams, $location, Api) {
    $scope.neu = !$routeParams.id;
    $scope.kunde = {};

    if (!$scope.neu) {
        Api.kunde($routeParams.id).then(function (res) {
            $scope.kunde = res.data;
        });
    }

    $scope.speichern = function () {
        Api.kundeSpeichern($scope.kunde).then(function (res) {
            zeigeMeldung('Kunde gespeichert', 'success');
            $location.path('/kunden/' + res.data.KdNr);
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };

    $scope.deaktivieren = function () {
        if (!confirm('Kunde wirklich deaktivieren?')) return;
        Api.kundeDeaktivieren($scope.kunde.KdNr).then(function () {
            $location.path('/kunden');
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };

    $scope.auftragAnlegen = function () {
        var beschreibung = prompt('Beschreibung des Auftrags:');
        if (!beschreibung) return;
        Api.auftragAnlegen($scope.kunde.KdNr, beschreibung).then(function (res) {
            $location.path('/auftraege/' + res.data.AuftragsNr);
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };
});

avApp.controller('AuftraegeCtrl', function ($scope, Api) {
    $scope.statusFilter = '';
    $scope.statusListe = ['Offen', 'In Arbeit', 'Erledigt', 'Abgerechnet'];

    $scope.laden = function () {
        Api.auftraege($scope.statusFilter).then(function (res) {
            $scope.auftraege = res.data;
        });
    };
    $scope.laden();
});

avApp.controller('AuftragCtrl', function ($scope, $routeParams, Api) {
    $scope.statusListe = ['Offen', 'In Arbeit', 'Erledigt'];
    $scope.neuePos = { Menge: 1, Einheit: 'Stk' };

    function laden() {
        Api.auftrag($routeParams.id).then(function (res) {
            $scope.auftrag = res.data;
            $scope.neuerStatus = res.data.Status;
            $scope.techniker = res.data.Techniker;
        });
    }
    laden();

    $scope.statusSpeichern = function () {
        Api.statusSetzen($scope.auftrag.AuftragsNr, $scope.neuerStatus, $scope.techniker).then(function () {
            zeigeMeldung('Status geändert', 'success');
            laden();
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };

    $scope.positionHinzufuegen = function () {
        Api.positionHinzufuegen($scope.auftrag.AuftragsNr, $scope.neuePos).then(function () {
            $scope.neuePos = { Menge: 1, Einheit: 'Stk' };
            laden();
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };

    $scope.positionLoeschen = function (pos) {
        Api.positionLoeschen($scope.auftrag.AuftragsNr, pos.PosId).then(laden);
    };

    $scope.rechnungErstellen = function () {
        Api.rechnungErstellen($scope.auftrag.AuftragsNr).then(function (res) {
            zeigeMeldung('Rechnung ' + res.data.RechNr + ' erstellt: ' + res.data.Brutto + ' EUR', 'success');
            laden();
        }, function (err) {
            zeigeMeldung(fehlertext(err), 'danger');
        });
    };
});

avApp.controller('RechnungenCtrl', function ($scope, Api) {
    $scope.nurOffene = false;

    $scope.laden = function () {
        Api.rechnungen($scope.nurOffene).then(function (res) {
            $scope.rechnungen = res.data;
            $scope.summeOffen = 0;
            for (var i = 0; i < res.data.length; i++) {
                if (!res.data[i].Bezahlt) $scope.summeOffen += res.data[i].Brutto;
            }
        });
    };

    $scope.bezahlt = function (r) {
        Api.alsBezahltMarkieren(r.RechNr).then($scope.laden);
    };

    $scope.laden();
});
