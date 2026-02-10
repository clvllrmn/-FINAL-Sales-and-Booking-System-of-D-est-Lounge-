app.controller("DestLoungeSalesandBookingController",
    function ($scope, $window, DestLoungeSalesandBookingService, $http, $httpParamSerializerJQLike) {

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
            if ($scope.selectedCategory === 'all') return true;
            return service.category === $scope.selectedCategory;
        };

        // FAQs data
        $scope.faqs = [
            { question: "What is your Opening Hours?", answer: "We are open Monday to Thursday from 9am to 7pm, and Friday to Sunday from 8am to 9pm.", isOpen: true },
            { question: "Do I need to make an appointment?", answer: "While walk-ins are welcome, we recommend making an appointment to ensure availability and minimize wait time.", isOpen: true },
            { question: "What is your last call?", answer: "An hour before closing time for a quick service like manicure and pedicure. 2 hours before closing time for longer services like intricate nail art,softgel, and extensions.", isOpen: true },
            { question: "What are your payment methods?", answer: "We accept payments in cash and mobile payment through Gcash.", isOpen: true },
            { question: "Do you accept walk-ins?", answer: "Yes! We do! But we highly recommend that you book an appointment to secure your time slot especially during the weekends.", isOpen: true },
            { question: "What is your booking policy?", answer: "A non-refundable down payment equivalent of 40% of the estimated cost is required to secure and confirm any booking.", isOpen: true }
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

        // Monday to Thursday: 9am to 7pm
        $scope.weekdayTimes = [
            "09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM", "07:00 PM"
        ];

        // Friday to Sunday: 8am to 9pm
        $scope.weekendTimes = [
            "08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM", "08:00 PM"
        ];

        $scope.availableTimes = [];

        // Set minimum date to today and maximum date to 1 month from today
        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0');
        var yyyy = today.getFullYear();
        $scope.minDate = yyyy + '-' + mm + '-' + dd;

        var maxDate = new Date();
        maxDate.setMonth(maxDate.getMonth() + 1);
        var maxDD = String(maxDate.getDate()).padStart(2, '0');
        var maxMM = String(maxDate.getMonth() + 1).padStart(2, '0');
        var maxYYYY = maxDate.getFullYear();
        $scope.maxDate = maxYYYY + '-' + maxMM + '-' + maxDD;

        $scope.fullyBookedDates = [];
        $scope.dateFullyBooked = false;

        $scope.checkAvailability = function () {
            if ($scope.booking.date) {
                var selectedDate;

                if ($scope.booking.date instanceof Date) {
                    selectedDate = $scope.booking.date;
                } else {
                    var parts = $scope.booking.date.split('-');
                    selectedDate = new Date(parts[0], parts[1] - 1, parts[2]);
                }

                var selectedDateStr = $scope.booking.date;
                if ($scope.booking.date instanceof Date) {
                    var year = selectedDate.getFullYear();
                    var month = String(selectedDate.getMonth() + 1).padStart(2, '0');
                    var day = String(selectedDate.getDate()).padStart(2, '0');
                    selectedDateStr = year + '-' + month + '-' + day;
                }

                $scope.dateFullyBooked = $scope.fullyBookedDates.indexOf(selectedDateStr) !== -1;

                if ($scope.dateFullyBooked) {
                    $scope.booking.time = '';
                    $scope.availableTimes = [];
                } else {
                    var dayOfWeek = selectedDate.getDay();
                    if (dayOfWeek === 0 || dayOfWeek === 5 || dayOfWeek === 6) {
                        $scope.availableTimes = $scope.weekendTimes;
                    } else {
                        $scope.availableTimes = $scope.weekdayTimes;
                    }

                    if ($scope.booking.time && $scope.availableTimes.indexOf($scope.booking.time) === -1) {
                        $scope.booking.time = '';
                    }
                }
            } else {
                $scope.availableTimes = [];
            }
        };

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

        $scope.calculateTotal = function () {
            var total = 0;
            angular.forEach($scope.booking.selectedServices, function (service) {
                total += service.price;
            });
            return total;
        };

        $scope.isBookingValid = function () {
            return $scope.booking.nailTech &&
                $scope.booking.date &&
                $scope.booking.time &&
                $scope.booking.selectedServices.length > 0 &&
                !$scope.dateFullyBooked;
        };

        // Submit booking (CONNECTED TO BACKEND)
        $scope.submitBooking = function () {
            if (!$scope.isBookingValid()) {
                alert("Please complete all booking fields.");
                return;
            }

            // date to yyyy-MM-dd
            var d = $scope.booking.date;
            var dateObj = (d instanceof Date) ? d : new Date(d);

            if (isNaN(dateObj.getTime())) {
                alert("Invalid date.");
                return;
            }

            var yyyy = dateObj.getFullYear();
            var mm = String(dateObj.getMonth() + 1).padStart(2, "0");
            var dd = String(dateObj.getDate()).padStart(2, "0");
            var bookingDate = yyyy + "-" + mm + "-" + dd;

            // Convert "09:00 AM" to 24h
            function to24h(t) {
                var parts = t.trim().split(" ");
                var hm = parts[0].split(":");
                var hour = parseInt(hm[0], 10);
                var min = parseInt(hm[1], 10);
                var ampm = (parts[1] || "").toUpperCase();

                if (ampm === "PM" && hour < 12) hour += 12;
                if (ampm === "AM" && hour === 12) hour = 0;

                return { hour: hour, min: min };
            }

            var start = to24h($scope.booking.time);
            var startHour = start.hour;
            var startMin = start.min;

            // +2 hours slot
            var endHour = startHour + 2;
            var endMin = startMin;
            if (endHour >= 24) endHour -= 24;

            var startTime = String(startHour).padStart(2, "0") + ":" + String(startMin).padStart(2, "0");
            var endTime = String(endHour).padStart(2, "0") + ":" + String(endMin).padStart(2, "0");

            // TEMP serviceId
            var serviceId = 1;

            var payload = {
                customerId: 1,
                serviceId: serviceId,
                bookingDate: bookingDate,
                startTime: startTime,
                endTime: endTime,
                nailTech: String($scope.booking.nailTech || ""),
                downpayment: String($scope.calculateTotal() || ""),
                notes: "Services: " + ($scope.booking.selectedServices || []).map(s => s.name).join(", ")
            };

            $http({
                method: "POST",
                url: "/Booking/Create",
                data: $httpParamSerializerJQLike(payload),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {
                var res = resp.data;
                if (res && res.success) {
                    alert("✅ Booking created. (ID=" + res.bookingId + ")");
                    $scope.booking.time = "";
                    $scope.bookingServices.forEach(function (s) { s.selected = false; });
                    $scope.updateSelectedServices();
                } else {
                    alert("❌ " + (res && res.message ? res.message : "Booking failed"));
                }
            }).catch(function (err) {
                console.error("Booking Create error:", err);
                alert("❌ Server error");
            });
        };

        // ===== STATUS UPDATE + CANCEL (BACKEND) =====

        $scope.updateBookingStatus = function (bookingId, newStatus) {
            var payload = { bookingId: bookingId, status: newStatus };

            return $http({
                method: "POST",
                url: "/Booking/UpdateStatus",
                data: $httpParamSerializerJQLike(payload),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {
                return resp.data;
            }).catch(function (err) {
                console.error(err);
                return { success: false, message: "Update failed" };
            });
        };

        $scope.cancelBooking = function (bookingId) {
            var reason = prompt("Cancel reason (optional):") || "";
            var payload = { bookingId: bookingId, reason: reason };

            return $http({
                method: "POST",
                url: "/Booking/Cancel",
                data: $httpParamSerializerJQLike(payload),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {
                return resp.data;
            }).catch(function (err) {
                console.error(err);
                return { success: false, message: "Cancel failed" };
            });
        };

        // ===== ADMIN BOOKINGS (READ + FILTER + ACTION BUTTONS) =====

        $scope.selectedFilter = 'all';
        $scope.bookings = [];
        $scope.filteredBookings = [];

        function mvcDateToJsDate(mvcDate) {
            if (!mvcDate) return null;
            var m = /\/Date\((\d+)\)\//.exec(mvcDate);
            if (m) return new Date(parseInt(m[1], 10));
            var d = new Date(mvcDate);
            return isNaN(d.getTime()) ? null : d;
        }

        function formatDateTime(row) {
            var d = mvcDateToJsDate(row.BookingDate);
            var dateStr = d ? d.toLocaleDateString() : "N/A";
            var st = row.StartTime ? String(row.StartTime).substring(0, 5) : "";
            var et = row.EndTime ? String(row.EndTime).substring(0, 5) : "";
            return dateStr + " " + st + "-" + et;
        }

        function extractServiceName(row) {
            if (row.Notes && row.Notes.indexOf("Services:") >= 0) {
                var after = row.Notes.split("Services:")[1].trim();
                var servicePart = after.split("|")[0].trim();
                return servicePart || ("ServiceId #" + row.ServiceId);
            }
            return "ServiceId #" + row.ServiceId;
        }

        $scope.loadBookings = function () {
            $http.get("/Booking/List").then(function (resp) {
                var rows = resp.data || [];

                $scope.bookings = rows.map(function (r) {
                    return {
                        BookingId: r.BookingId,
                        CustomerId: r.CustomerId,
                        ServiceId: r.ServiceId,
                        Status: r.Status,
                        Notes: r.Notes,
                        clientName: "Customer #" + r.CustomerId,
                        service: extractServiceName(r),
                        dateTime: formatDateTime(r),
                        contact: "N/A"
                    };
                });

                $scope.filterBookings($scope.selectedFilter);
            }).catch(function (err) {
                console.error("loadBookings error:", err);
            });
        };

        $scope.filterBookings = function (filter) {
            $scope.selectedFilter = filter;

            if (filter === 'all') {
                $scope.filteredBookings = $scope.bookings;
                return;
            }

            if (filter === 'today') {
                var todayStr = new Date().toLocaleDateString();
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    return b.dateTime && b.dateTime.indexOf(todayStr) >= 0;
                });
                return;
            }

            if (filter === 'upcoming') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    return b.Status !== "Completed" && b.Status !== "Cancelled";
                });
                return;
            }

            if (filter === 'completed') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    return b.Status === "Completed";
                });
                return;
            }

            $scope.filteredBookings = $scope.bookings;
        };

        $scope.approveBooking = function (bookingId) {
            if (!confirm("Approve this booking?")) return;
            $scope.updateBookingStatus(bookingId, "Approved").then(function (res) {
                alert(res.message || "Approved.");
                $scope.loadBookings();
            });
        };

        $scope.markAsComplete = function (bookingId) {
            if (!confirm("Mark this booking as Completed?")) return;
            $scope.updateBookingStatus(bookingId, "Completed").then(function (res) {
                alert(res.message || "Completed.");
                $scope.loadBookings();
            });
        };

        $scope.cancelBookingAdmin = function (bookingId) {
            if (!confirm("Cancel this booking?")) return;
            $scope.cancelBooking(bookingId).then(function (res) {
                alert(res.message || "Cancelled.");
                $scope.loadBookings();
            });
        };

        // Load bookings automatically (for AdminBookingPage)
        setTimeout(function () {
            $scope.loadBookings();
        }, 0);

        // ===== EXISTING FUNCTIONS =====

        $scope.openTerms = function (event) {
            if (event) {
                event.preventDefault();
                event.stopPropagation();
            }
            $scope.showTerms = true;
        };

        $scope.closeTerms = function () {
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
        scope: { otherModelValue: "=compareTo" },
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
