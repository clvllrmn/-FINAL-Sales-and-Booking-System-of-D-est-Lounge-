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

        // ===================== ADMIN BOOKING LIST (LOAD + FILTER + UI STATE) =====================

        // Holds all bookings from backend
        $scope.bookings = [];
        $scope.filteredBookings = [];
        $scope.selectedFilter = 'all';

        // Helper: parse ASP.NET JSON date "/Date(1770...)/"
        function parseNetDate(netDateStr) {
            if (!netDateStr) return null;
            // supports "/Date(177069...) /"
            var match = /\/Date\((\d+)\)\//.exec(netDateStr);
            if (match && match[1]) return new Date(parseInt(match[1], 10));
            return new Date(netDateStr); // fallback
        }

        function pad2(n) { return String(n).padStart(2, "0"); }

        function formatDateTime(d, startTime, endTime) {
            // d: Date
            if (!d) return "N/A";
            var day = pad2(d.getDate());
            var mon = pad2(d.getMonth() + 1);
            var yr = d.getFullYear();

            // startTime/endTime from DB are like "10:00:00"
            var st = (startTime || "").substring(0, 5);
            var et = (endTime || "").substring(0, 5);

            return `${day}/${mon}/${yr} ${st}-${et}`;
        }

        // Extract cancel reason from Notes (since you append: " | Cancel reason: ...")
        function extractCancelReason(notes) {
            if (!notes) return "";
            var key = "Cancel reason:";
            var idx = notes.indexOf(key);
            if (idx === -1) return "";
            return notes.substring(idx + key.length).trim();
        }

        // Load list from backend (you already have /booking/List working)
        $scope.loadBookings = function () {
            return $http.get("/booking/List").then(function (resp) {
                var rows = resp.data || [];

                // Map backend shape -> UI shape used by AdminBookingPage.cshtml
                $scope.bookings = rows.map(function (b) {
                    var dateObj = parseNetDate(b.BookingDate);
                    var status = (b.Status || "Pending").trim();

                    // Service label: if you still don’t join tbl_services, show fallback
                    // Your Notes currently contain "Services: X | NailTech: Y | Downpayment: Z"
                    var serviceLabel = "ServiceId #" + b.ServiceId;
                    if (b.Notes && b.Notes.indexOf("Services:") !== -1) {
                        // quick extract after "Services:"
                        var s = b.Notes.split("|")[0]; // "Services: ..."
                        serviceLabel = s.replace("Services:", "").trim();
                    }

                    return {
                        // keep original ids
                        bookingId: b.BookingId,

                        // UI fields your cshtml uses
                        clientName: "Customer #" + b.CustomerId,
                        service: serviceLabel,
                        dateTime: formatDateTime(dateObj, b.StartTime, b.EndTime),
                        contact: b.Notes ? b.Notes : "N/A",

                        // status
                        status: status,

                        // cancel reason for display
                        cancelReason: extractCancelReason(b.Notes),

                        // keep raw if needed later
                        _raw: b
                    };
                });

                // Apply current filter
                $scope.filterBookings($scope.selectedFilter);
            }).catch(function (err) {
                console.error("loadBookings error:", err);
                alert("❌ Failed to load bookings. Check console.");
            });
        };

        // Filters
        $scope.filterBookings = function (filter) {
            $scope.selectedFilter = filter;

            var today = new Date();
            today.setHours(0, 0, 0, 0);

            function parseCardDateTime(card) {
                // card.dateTime = "dd/MM/yyyy HH:mm-HH:mm"
                // we’ll parse the date part only
                if (!card || !card.dateTime || card.dateTime === "N/A") return null;
                var datePart = card.dateTime.split(" ")[0]; // dd/MM/yyyy
                var parts = datePart.split("/");
                if (parts.length !== 3) return null;
                var dd = parseInt(parts[0], 10);
                var mm = parseInt(parts[1], 10);
                var yy = parseInt(parts[2], 10);
                var d = new Date(yy, mm - 1, dd);
                d.setHours(0, 0, 0, 0);
                return d;
            }

            if (filter === 'all') {
                // ✅ THIS IS THE IMPORTANT FIX: include Cancelled in history
                $scope.filteredBookings = $scope.bookings;

            } else if (filter === 'today') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    var d = parseCardDateTime(b);
                    return d && d.getTime() === today.getTime();
                });

            } else if (filter === 'upcoming') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    var d = parseCardDateTime(b);
                    // upcoming: future dates, and not cancelled
                    return d && d.getTime() > today.getTime() && b.status !== 'Cancelled';
                });

            } else if (filter === 'completed') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    return b.status === 'Completed';
                });

            } else if (filter === 'cancelled') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    return b.status === 'Cancelled';
                });
            }
        };

        // Call this once when admin page loads
        // (safe even if you open other pages; it will just fail silently if endpoint not reachable)
        if (window.location.pathname.toLowerCase().indexOf("adminbookingpage") !== -1) {
            $scope.loadBookings();
        }


        // ===== STATUS UPDATE + CANCEL (BACKEND) =====

        $scope.updateBookingStatus = function (bookingId, newStatus) {
            var payload = { bookingId: bookingId, status: newStatus };

            return $http({
                method: "POST",
                url: "/Booking/UpdateStatus",
                data: $httpParamSerializerJQLike(payload),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {

                // ✅ Update locally so it stays visible + badge changes
                var card = $scope.bookings.find(b => b.bookingId === bookingId);
                if (card) card.status = newStatus;

                // Re-apply filter (so it moves to Completed tab etc.)
                $scope.filterBookings($scope.selectedFilter);

                alert(resp.data.message || "Updated.");
                return resp.data;

            }).catch(function (err) {
                console.error(err);
                alert("❌ Update failed");
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

                // ✅ Update locally so it DOESN’T disappear
                var card = $scope.bookings.find(b => b.bookingId === bookingId);
                if (card) {
                    card.status = "Cancelled";
                    card.cancelReason = reason;
                    // optional: also append to contact/notes display
                    if (reason) card.contact = (card.contact || "") + " | Cancel reason: " + reason;
                }

                $scope.filterBookings($scope.selectedFilter);

                alert(resp.data.message || "Cancelled.");
                return resp.data;

            }).catch(function (err) {
                console.error(err);
                alert("❌ Cancel failed");
            });
        };


        

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

        // ===================== SALES ANALYTICS (Completed bookings revenue) =====================

        $scope.salesRange = "week";
        $scope.sales = { totalRevenue: 0, totalBookings: 0, points: [], range: "week" };

        var salesChartInstance = null;

        $scope.setSalesRange = function (range) {
            $scope.salesRange = range;
            $scope.loadSalesAnalytics(range);
        };

        $scope.loadSalesAnalytics = function (range) {
            range = range || $scope.salesRange || "week";

            return $http.get("/Sales/Analytics", { params: { range: range } })
                .then(function (resp) {
                    $scope.sales = resp.data || {};
                    $scope.renderSalesChart($scope.sales.points || []);
                })
                .catch(function (err) {
                    console.error("Sales analytics error:", err);
                    alert("❌ Failed to load Sales analytics. Check console.");
                });
        };

        $scope.renderSalesChart = function (points) {
            // Needs Chart.js loaded
            if (!window.Chart) {
                console.warn("Chart.js not found. Load Chart.js in _MainLayout or this page.");
                return;
            }

            var labels = (points || []).map(p => p.label);
            var data = (points || []).map(p => Number(p.value || 0));

            var canvas = document.getElementById("salesChart");
            if (!canvas) return;

            var ctx = canvas.getContext("2d");

            if (salesChartInstance) {
                salesChartInstance.destroy();
                salesChartInstance = null;
            }

            salesChartInstance = new Chart(ctx, {
                type: "line",
                data: {
                    labels: labels,
                    datasets: [{
                        label: "Total Revenue",
                        data: data,
                        tension: 0.35,
                        fill: false,
                        pointRadius: 4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: true }
                    },
                    scales: {
                        y: { beginAtZero: true }
                    }
                }
            });
        };

        // Auto-load ONLY on AdminSalesPage
        if (window.location.pathname.toLowerCase().indexOf("adminsalespage") !== -1) {
            setTimeout(function () {
                $scope.$applyAsync(function () {
                    $scope.loadSalesAnalytics($scope.salesRange);
                });
            }, 0);
        }

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


