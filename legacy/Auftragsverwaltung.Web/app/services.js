avApp.factory('Api', function ($http) {
    var basis = 'api/';

    return {
        login: function (login, passwort) {
            return $http.post(basis + 'login', { Login: login, Passwort: passwort });
        },

        // Kunden
        kunden: function (suche) {
            return $http.get(basis + 'kunden', { params: { suche: suche } });
        },
        kunde: function (id) {
            return $http.get(basis + 'kunden/' + id);
        },
        kundeSpeichern: function (kunde) {
            if (kunde.KdNr) {
                return $http.put(basis + 'kunden/' + kunde.KdNr, kunde);
            }
            return $http.post(basis + 'kunden', kunde);
        },
        kundeDeaktivieren: function (id) {
            return $http['delete'](basis + 'kunden/' + id);
        },

        // Aufträge
        auftraege: function (status) {
            return $http.get(basis + 'auftraege', { params: { status: status } });
        },
        auftrag: function (id) {
            return $http.get(basis + 'auftraege/' + id);
        },
        auftragAnlegen: function (kdNr, beschreibung) {
            return $http.post(basis + 'auftraege', { KdNr: kdNr, Beschreibung: beschreibung });
        },
        statusSetzen: function (id, status, techniker) {
            return $http.put(basis + 'auftraege/' + id + '/status', { Status: status, Techniker: techniker });
        },
        positionHinzufuegen: function (id, pos) {
            return $http.post(basis + 'auftraege/' + id + '/positionen', pos);
        },
        positionLoeschen: function (id, posId) {
            return $http['delete'](basis + 'auftraege/' + id + '/positionen/' + posId);
        },
        rechnungErstellen: function (id) {
            return $http.post(basis + 'auftraege/' + id + '/rechnung');
        },

        // Rechnungen
        rechnungen: function (nurOffene) {
            return $http.get(basis + 'rechnungen', { params: { offen: nurOffene } });
        },
        alsBezahltMarkieren: function (rechNr) {
            return $http.put(basis + 'rechnungen/' + rechNr + '/bezahlt');
        }
    };
});
