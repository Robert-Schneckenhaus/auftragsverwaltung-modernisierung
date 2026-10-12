var avApp = angular.module('avApp', ['ngRoute']);

avApp.config(function ($routeProvider, $locationProvider) {
    $locationProvider.hashPrefix('');

    $routeProvider
        .when('/login', { templateUrl: 'app/views/login.html', controller: 'LoginCtrl' })
        .when('/kunden', { templateUrl: 'app/views/kunden.html', controller: 'KundenCtrl' })
        .when('/kunden/neu', { templateUrl: 'app/views/kunde.html', controller: 'KundeCtrl' })
        .when('/kunden/:id', { templateUrl: 'app/views/kunde.html', controller: 'KundeCtrl' })
        .when('/auftraege', { templateUrl: 'app/views/auftraege.html', controller: 'AuftraegeCtrl' })
        .when('/auftraege/:id', { templateUrl: 'app/views/auftrag.html', controller: 'AuftragCtrl' })
        .when('/rechnungen', { templateUrl: 'app/views/rechnungen.html', controller: 'RechnungenCtrl' })
        .otherwise({ redirectTo: '/auftraege' });
});

avApp.run(function ($rootScope, $location) {
    var gespeichert = localStorage.getItem('benutzer');
    if (gespeichert) {
        $rootScope.benutzer = JSON.parse(gespeichert);
    }

    // "Schutz" der Seiten nur im Frontend
    $rootScope.$on('$routeChangeStart', function (event, next) {
        if (!$rootScope.benutzer && next.$$route && next.$$route.originalPath != '/login') {
            $location.path('/login');
        }
    });

    $rootScope.abmelden = function () {
        localStorage.removeItem('benutzer');
        $rootScope.benutzer = null;
        $location.path('/login');
    };
});

// globale Hilfsfunktion, wird aus mehreren Controllern aufgerufen
function zeigeMeldung(text, typ) {
    $('#meldung').removeClass('alert-info alert-danger alert-success')
        .addClass('alert-' + (typ || 'info'))
        .text(text).fadeIn();
    setTimeout(function () { $('#meldung').fadeOut(); }, 3000);
}
