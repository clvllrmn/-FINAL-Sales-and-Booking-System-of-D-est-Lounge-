// If you already have an angular module, DO NOT redefine it.
// Use angular.module("YOUR_APP_NAME") instead.
// If you don't know, search your solution for: angular.module(
var app;
try {
    app = angular.module("destLoungeApp");
} catch (e) {
    app = angular.module("destLoungeApp", []);
}

app.controller("DestLoungeSalesandBookingController", function ($scope, $http, $httpParamSerializerJQLike) {

    // ------- sample data (replace later with real API if your groupmate builds services/users) -------
    $scope.nailTechs = $scope.nailTechs || [
        { id: 1, name: "Nail Tech 1" },
        { id: 2, name: "Nail Tech 2" }
    ];

    // If your UI already sets bookingServices somewhere else, keep it.
    $scope.bookingServices = $scope.bookingServices || [
        { id: 1, name: "Manicure", price: 99, selected: false },
        { id: 2, name: "Pedicure", price: 139, selected: false }
    ];

    // Available time slots (you can later generate based on availability)
    $scope.availableTimes = $scope.availableTimes || [
        "10:00 - 11:00",
        "11:00 - 12:00",
        "12:00 - 13:00",
        "13:00 - 14:00",
        "14:00 - 15:00",
    ];

    // ------- booking model -------
    $scope.booking = $scope.booking || {
        nailTech: "",
        date: null,
        time: "",
        selectedServices: []
    };

    // ------- helpers -------
    $scope.calculateTotal = function () {
        return ($scope.booking.selectedServices || []).reduce((sum, s) => sum + (Number(s.price) || 0), 0);
    };

    $scope.updateSelectedServices = function () {
        $scope.booking.selectedServices = $scope.bookingServices.filter(s => s.selected);
    };

    $scope.isBookingValid = function () {
        return !!$scope.booking.nailTech &&
            !!$scope.booking.date &&
            !!$scope.booking.time &&
            ($scope.booking.selectedServices && $scope.booking.selectedServices.length > 0);
    };

    function formatDateYYYYMMDD(d) {
        // Angular date input gives Date object
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const dd = String(d.getDate()).padStart(2, "0");
        return `${yyyy}-${mm}-${dd}`;
    }

    function parseSlot(slotText) {
        // "11:00 - 12:00" -> { startTime:"11:00", endTime:"12:00" }
        const parts = (slotText || "").split("-");
        if (parts.length !== 2) return null;
        return {
            startTime: parts[0].trim(),
            endTime: parts[1].trim()
        };
    }

    // Optional: you can keep this for now (no backend call yet)
    $scope.dateFullyBooked = false;
    $scope.checkAvailability = function () {
        // Later, call /Booking/AvailableTimes?date=yyyy-mm-dd
        $scope.dateFullyBooked = false;
    };

    // ------- MAIN: submit booking to backend -------
    $scope.submitBooking = function () {
        if (!$scope.isBookingValid()) {
            alert("Please complete all booking fields.");
            return;
        }

        $scope.updateSelectedServices();

        const mainService = $scope.booking.selectedServices[0];
        const slot = parseSlot($scope.booking.time);

        if (!slot) {
            alert("Invalid time slot.");
            return;
        }

        // bookingDate must be yyyy-MM-dd
        const bookingDate = formatDateYYYYMMDD($scope.booking.date);

        const payload = {
            customerId: 1, // TEMP
            serviceId: mainService.id,
            bookingDate: bookingDate,
            startTime: slot.startTime, // e.g. "11:00"
            endTime: slot.endTime,     // e.g. "12:00"
            nailTech: String($scope.booking.nailTech || ""),
            downpayment: String($scope.calculateTotal() || ""),
            notes: "Services: " + $scope.booking.selectedServices.map(s => s.name).join(", ")
        };

        console.log("POST /Booking/Create payload:", payload);

        $http({
            method: "POST",
            url: "/Booking/Create",
            data: $httpParamSerializerJQLike(payload),
            headers: { "Content-Type": "application/x-www-form-urlencoded" }
        }).then(function (resp) {
            console.log("Create response:", resp.data);

            const res = resp.data;
            if (res && res.success) {
                alert("✅ " + res.message + " (ID=" + res.bookingId + ")");

                // reset
                $scope.booking.time = "";
                $scope.bookingServices.forEach(s => s.selected = false);
                $scope.updateSelectedServices();
            } else {
                alert("❌ " + (res && res.message ? res.message : "Booking failed"));
            }
        }).catch(function (err) {
            console.error("Create error:", err);
            alert("❌ Server error. Check DevTools → Network → POST /Booking/Create");
        });
    };

})

