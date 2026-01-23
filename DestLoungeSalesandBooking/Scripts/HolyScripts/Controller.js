app.controller("DestLoungeSalesandBookingController", function ($scope, $window, DestLoungeSalesandBookingService, $http) {
    $scope.user = {};
    $scope.showPassword = false;
    $scope.showTerms = false;

    // Services data for Services Page
    $scope.services = [
        { name: "Gel Manicure", image: "~/Content/Pictures/service1.jpg", category: "manicure" },
        { name: "French Manicure", image: "~/Content/Pictures/service2.jpg", category: "manicure" },
        { name: "Nail Art", image: "~/Content/Pictures/service3.jpg", category: "manicure" },
        { name: "Classic Pedicure", image: "~/Content/Pictures/service4.jpg", category: "pedicure" },
        { name: "Spa Pedicure", image: "~/Content/Pictures/service5.jpg", category: "pedicure" },
        { name: "Gel Pedicure", image: "~/Content/Pictures/service6.jpg", category: "pedicure" }
    ];

    $scope.selectedCategory = 'all';

    $scope.filterServices = function (category) {
        $scope.selectedCategory = category;
    };

    $scope.categoryFilter = function (service) {
        if ($scope.selectedCategory === 'all') {
            return true;
        }
        return service.category === $scope.selectedCategory;
    };

    // FAQs data
    $scope.faqs = [
        {
            question: "What is your Opening Hours?",
            answer: "We are open Monday to Thursday from 9am to 7pm, and Friday to Sunday from 8am to 9pm.",
            isOpen: true
        },
        {
            question: "Do I need to make an appointment?",
            answer: "While walk-ins are welcome, we recommend making an appointment to ensure availability and minimize wait time.",
            isOpen: true
        },
        {
            question: "What is your last call?",
            answer: "An hour before closing time for a quick service like manicure and pedicure. 2 hours before closing time for longer services like intricate nail art,softgel, and extensions.",
            isOpen: true
        },
        {
            question: "What are your payment methods?",
            answer: "We accept payments in cash and mobile payment through Gcash.",
            isOpen: true
        },
        {
            question: "Do you accept walk-ins?",
            answer: "Yes! We do! But we highly recommend that you book an appointment to secure your time slot especially during the weekends.",
            isOpen: true
        },
        {
            question: "What is your booking policy?",
            answer: "A non-refundable down payment equivalent of 40% of the estimated cost is required to secure and confirm any booking.",
            isOpen: true
        }
    ];

    $scope.toggleFaq = function (index) {
        $scope.faqs[index].isOpen = !$scope.faqs[index].isOpen;
    };

    // ===== BOOKING PAGE DATA =====

    // Nail Technicians
    $scope.nailTechs = [
        { id: 1, name: "Name 1" },
        { id: 2, name: "Name 2" },
        { id: 3, name: "Name 3" }
    ];

    // Services with prices for Booking Page
    $scope.bookingServices = [
        { name: "Manicure", price: 99, selected: false },
        { name: "Pedicure", price: 139, selected: false },
        { name: "Add: Branded Polish", price: 20, selected: false },
        { name: "Pedicure Gel", price: 479, selected: false },
        { name: "Manicure Gel", price: 379, selected: false },
        { name: "BIAB Overlay Gel", price: 999, selected: false },
        { name: "Soft Gel-2 (Medium)", price: 599, selected: false },
        { name: "Soft Gel-3 (Long)", price: 699, selected: false }
    ];

    // Booking object
    $scope.booking = {
        nailTech: '',
        date: '',
        time: '',
        selectedServices: []
    };

    // Time slots for different days - CHANGED TO $scope SO THEY'RE ACCESSIBLE
    // Monday to Thursday: 9am to 7pm
    $scope.weekdayTimes = [
        "09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM", "07:00 PM"
    ];

    // Friday to Sunday: 8am to 9pm
    $scope.weekendTimes = [
        "08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM","08:00 PM"
    ];

    // Initialize with empty times
    $scope.availableTimes = [];

    // Set minimum date to today and maximum date to 1 month from today
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();
    $scope.minDate = yyyy + '-' + mm + '-' + dd;

    // Calculate max date (1 month from today)
    var maxDate = new Date();
    maxDate.setMonth(maxDate.getMonth() + 1);
    var maxDD = String(maxDate.getDate()).padStart(2, '0');
    var maxMM = String(maxDate.getMonth() + 1).padStart(2, '0');
    var maxYYYY = maxDate.getFullYear();
    $scope.maxDate = maxYYYY + '-' + maxMM + '-' + maxDD;

    // Fully booked dates (this would come from backend in real app)
    $scope.fullyBookedDates = [];

    $scope.dateFullyBooked = false;

    // Check if selected date is available and update time slots - UPDATED WITH DEBUGGING
    $scope.checkAvailability = function () {
        console.log("=== CHECK AVAILABILITY CALLED ===");
        console.log("booking.date value:", $scope.booking.date);
        console.log("booking.date type:", typeof $scope.booking.date);

        if ($scope.booking.date) {
            // Try to create date object
            var selectedDate;

            // Check if it's already a Date object
            if ($scope.booking.date instanceof Date) {
                selectedDate = $scope.booking.date;
                console.log("Date is already a Date object");
            } else {
                // It's a string, parse it
                var parts = $scope.booking.date.split('-');
                console.log("Date parts:", parts);
                selectedDate = new Date(parts[0], parts[1] - 1, parts[2]);
                console.log("Created date from parts");
            }

            console.log("Selected Date object:", selectedDate);
            console.log("Day of week:", selectedDate.getDay());
            console.log("Is valid date?", !isNaN(selectedDate.getTime()));

            // Format as YYYY-MM-DD for comparison
            var selectedDateStr = $scope.booking.date;
            if ($scope.booking.date instanceof Date) {
                var year = selectedDate.getFullYear();
                var month = String(selectedDate.getMonth() + 1).padStart(2, '0');
                var day = String(selectedDate.getDate()).padStart(2, '0');
                selectedDateStr = year + '-' + month + '-' + day;
            }
            console.log("Date string for comparison:", selectedDateStr);

            // Check if date is fully booked
            $scope.dateFullyBooked = $scope.fullyBookedDates.indexOf(selectedDateStr) !== -1;
            console.log("Date fully booked?", $scope.dateFullyBooked);

            if ($scope.dateFullyBooked) {
                $scope.booking.time = '';
                $scope.availableTimes = [];
                console.log("Date is fully booked - no times available");
            } else {
                // Get day of week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
                var dayOfWeek = selectedDate.getDay();
                console.log("Day of week number:", dayOfWeek);

                // Friday (5), Saturday (6), Sunday (0) = 8am to 9pm
                // Monday (1) to Thursday (4) = 9am to 7pm
                if (dayOfWeek === 0 || dayOfWeek === 5 || dayOfWeek === 6) {
                    // Friday to Sunday
                    $scope.availableTimes = $scope.weekendTimes;
                    console.log("WEEKEND TIMES SELECTED (Fri-Sun: 8am-9pm)");
                } else {
                    // Monday to Thursday
                    $scope.availableTimes = $scope.weekdayTimes;
                    console.log("WEEKDAY TIMES SELECTED (Mon-Thu: 9am-7pm)");
                }

                console.log("Available times array:", $scope.availableTimes);
                console.log("Available times length:", $scope.availableTimes.length);

                // Reset time if previously selected time is not available for new date
                if ($scope.booking.time && $scope.availableTimes.indexOf($scope.booking.time) === -1) {
                    $scope.booking.time = '';
                    console.log("Reset time selection");
                }
            }
        } else {
            console.log("No date selected");
            $scope.availableTimes = [];
        }

        console.log("=== END CHECK AVAILABILITY ===");
    };

    // Update selected services when checkboxes change
    $scope.updateSelectedServices = function () {
        $scope.booking.selectedServices = [];

        angular.forEach($scope.bookingServices, function (service) {
            if (service.selected) {
                $scope.booking.selectedServices.push({
                    name: service.name,
                    price: service.price
                });
            }
        });
    };

    // Calculate total price
    $scope.calculateTotal = function () {
        var total = 0;
        angular.forEach($scope.booking.selectedServices, function (service) {
            total += service.price;
        });
        return total;
    };

    // Check if booking is valid
    $scope.isBookingValid = function () {
        return $scope.booking.nailTech &&
            $scope.booking.date &&
            $scope.booking.time &&
            $scope.booking.selectedServices.length > 0 &&
            !$scope.dateFullyBooked;
    };

    // Submit booking
    $scope.submitBooking = function () {
        if ($scope.isBookingValid()) {
            var bookingData = {
                nailTech: $scope.booking.nailTech,
                date: $scope.booking.date,
                time: $scope.booking.time,
                services: $scope.booking.selectedServices,
                total: $scope.calculateTotal()
            };

            console.log("Booking submitted:", bookingData);

            // Here you would call your backend service
            // DestLoungeSalesandBookingService.createBooking(bookingData).then(function(response) {
            //     alert("Booking confirmed!");
            //     // Reset form or redirect
            // });

            alert("Booking confirmed! Total: ₱" + $scope.calculateTotal());
        }
    };

    // ===== EXISTING FUNCTIONS =====

    $scope.openTerms = function (event) {
        console.log("Terms clicked!");
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }
        $scope.showTerms = true;
    };

    $scope.closeTerms = function () {
        console.log("Closing terms");
        $scope.showTerms = false;
    };

    $scope.togglePassword = function () {
        $scope.showPassword = !$scope.showPassword;
    };

    $scope.focusNext = function (event, nextId) {
        if (event.key === "Enter") {
            event.preventDefault();
            document.getElementById(nextId).focus();
        }
    };

    $scope.submitOnEnter = function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            if (!$scope.registerForm.$invalid) {
                document.querySelector(".signup-button").click();
            }
        }
    };
});

app.directive('compareTo', function () {
    return {
        require: "ngModel",
        scope: {
            otherModelValue: "=compareTo"
        },
        link: function (scope, element, attributes, ngModel) {
            ngModel.$validators.compareTo = function (value) {
                return value === scope.otherModelValue;
            };
            scope.$watch("otherModelValue", function () {
                ngModel.$validate();
            });
        }
    };
});