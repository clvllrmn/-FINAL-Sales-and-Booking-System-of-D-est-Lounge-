app.controller("DestLoungeSalesandBookingController",
    function ($scope, $window, DestLoungeSalesandBookingService, $http, $httpParamSerializerJQLike) {

        $scope.user = {};
        $scope.showPassword = false;
        $scope.showTerms = false;

        // ===== CONTACT PAGE - LOAD FROM DATABASE =====
        $scope.contactInfo = [];

        $scope.loadContactInfo = function () {
            $http.get('/Contact/GetAllContact')
                .then(function (response) {
                    if (response.data.success) {
                        $scope.contactInfo = response.data.data.map(function (contact) {
                            return {
                                contactID: contact.contactID,
                                infoType: contact.infoType,
                                label: contact.label,
                                value: contact.value,
                                icon: contact.icon
                            };
                        });
                        console.log('Contact info loaded:', $scope.contactInfo);
                    } else {
                        console.error('Failed to load contact info:', response.data.message);
                        alert('Failed to load contact info');
                    }
                })
                .catch(function (error) {
                    console.error('Error loading contact info:', error);
                    alert('Error loading contact info. Please refresh the page.');
                });
        };

        // ===== CONTACT CRUD =====

        $scope.addNewContact = function () {
            console.log('Add contact clicked');
            var infoType = prompt('Enter Info Type (address, hours, phone, email):');
            if (!infoType) return;

            var label = prompt('Enter Label (e.g., "Find us at"):');
            if (!label) return;

            var value = prompt('Enter Value (contact information):');
            if (!value) return;

            var icon = prompt('Enter Font Awesome Icon Class (e.g., "fa-solid fa-location-dot"):');
            if (!icon) return;

            $http({
                method: 'POST',
                url: '/Contact/CreateContact',
                data: $httpParamSerializerJQLike({
                    infoType: infoType.trim(),
                    label: label.trim(),
                    value: value.trim(),
                    icon: icon.trim()
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert('Contact info created successfully!');
                        $scope.loadContactInfo();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error creating contact info:', error);
                    alert('Error creating contact info');
                });
        };

        $scope.editContact = function (index) {
            console.log('Edit contact clicked, index:', index);
            var contact = $scope.contactInfo[index];

            var infoType = prompt('Edit Info Type:', contact.infoType);
            if (infoType === null) return;

            var label = prompt('Edit Label:', contact.label);
            if (label === null) return;

            var value = prompt('Edit Value:', contact.value);
            if (value === null) return;

            var icon = prompt('Edit Icon Class:', contact.icon);
            if (icon === null) return;

            $http({
                method: 'POST',
                url: '/Contact/UpdateContact/' + contact.contactID,
                data: $httpParamSerializerJQLike({
                    infoType: infoType.trim(),
                    label: label.trim(),
                    value: value.trim(),
                    icon: icon.trim()
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert('Contact info updated successfully!');
                        $scope.loadContactInfo();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error updating contact info:', error);
                    alert('Error updating contact info');
                });
        };

        $scope.deleteContact = function (index) {
            console.log('Delete contact clicked, index:', index);
            var contact = $scope.contactInfo[index];

            if (confirm('Are you sure you want to delete this contact info?')) {
                $http({
                    method: 'POST',
                    url: '/Contact/DeleteContact/' + contact.contactID,
                    data: {},
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                })
                    .then(function (response) {
                        if (response.data.success) {
                            alert('Contact info deleted successfully!');
                            $scope.loadContactInfo();
                        } else {
                            alert('Error: ' + response.data.message);
                        }
                    })
                    .catch(function (error) {
                        console.error('Error deleting contact info:', error);
                        alert('Error deleting contact info');
                    });
            }
        };

        // Load contact info when page loads
        var currentPath = window.location.pathname.toLowerCase();
        if (currentPath.indexOf("admincontactpage") !== -1 || currentPath.indexOf("contactpage") !== -1) {
            $scope.loadContactInfo();
        }

        // ===== SERVICES PAGE =====
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

        // ===== FAQs - LOAD FROM DATABASE =====
        $scope.faqs = [];

        $scope.loadFAQs = function () {
            $http.get('/FAQ/GetAllFAQs')
                .then(function (response) {
                    if (response.data.success) {
                        $scope.faqs = response.data.data.map(function (faq) {
                            return {
                                faqId: faq.faqId,
                                question: faq.question,
                                answer: faq.answer,
                                isOpen: false
                            };
                        });
                    } else {
                        console.error('Failed to load FAQs:', response.data.message);
                        alert('Failed to load FAQs');
                    }
                })
                .catch(function (error) {
                    console.error('Error loading FAQs:', error);
                    alert('Error loading FAQs. Please refresh the page.');
                });
        };

        $scope.toggleFaq = function (index) {
            $scope.faqs[index].isOpen = !$scope.faqs[index].isOpen;
        };

        // ===== FAQ CRUD =====

        $scope.addNewFaq = function () {
            var question = prompt('Enter FAQ Question:');
            if (!question) return;

            var answer = prompt('Enter FAQ Answer:');
            if (!answer) return;

            $http({
                method: 'POST',
                url: '/FAQ/CreateFAQ',
                data: $httpParamSerializerJQLike({ question: question.trim(), answer: answer.trim() }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert('FAQ created successfully!');
                        $scope.loadFAQs();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error creating FAQ:', error);
                    alert('Error creating FAQ');
                });
        };

        $scope.editFaq = function (index) {
            var faq = $scope.faqs[index];

            var question = prompt('Edit FAQ Question:', faq.question);
            if (question === null) return;

            var answer = prompt('Edit FAQ Answer:', faq.answer);
            if (answer === null) return;

            $http({
                method: 'POST',
                url: '/FAQ/UpdateFAQ/' + faq.faqId,
                data: $httpParamSerializerJQLike({ question: question.trim(), answer: answer.trim() }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert('FAQ updated successfully!');
                        $scope.loadFAQs();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error updating FAQ:', error);
                    alert('Error updating FAQ');
                });
        };

        $scope.deleteFaq = function (index) {
            var faq = $scope.faqs[index];

            if (confirm('Are you sure you want to delete this FAQ?')) {
                $http({
                    method: 'POST',
                    url: '/FAQ/DeleteFAQ/' + faq.faqId,
                    data: {},
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                })
                    .then(function (response) {
                        if (response.data.success) {
                            alert('FAQ deleted successfully!');
                            $scope.loadFAQs();
                        } else {
                            alert('Error: ' + response.data.message);
                        }
                    })
                    .catch(function (error) {
                        console.error('Error deleting FAQ:', error);
                        alert('Error deleting FAQ');
                    });
            }
        };

        // Load FAQs on init
        $scope.loadFAQs();

        // ===== BOOKING PAGE =====

        $scope.nailTechs = [
            { id: 1, name: "Name 1" },
            { id: 2, name: "Name 2" },
            { id: 3, name: "Name 3" }
        ];

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

        $scope.booking = {
            nailTech: '',
            date: '',
            time: '',
            selectedServices: []
        };

        $scope.weekdayTimes = [
            "09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM", "07:00 PM"
        ];

        $scope.weekendTimes = [
            "08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM", "08:00 PM"
        ];

        $scope.availableTimes = [];

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

        $scope.submitBooking = function () {
            if (!$scope.isBookingValid()) {
                alert("Please complete all booking fields.");
                return;
            }

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
            var endHour = startHour + 2;
            var endMin = startMin;
            if (endHour >= 24) endHour -= 24;

            var startTime = String(startHour).padStart(2, "0") + ":" + String(startMin).padStart(2, "0");
            var endTime = String(endHour).padStart(2, "0") + ":" + String(endMin).padStart(2, "0");

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

        // ===== ADMIN BOOKING LIST =====

        $scope.bookings = [];
        $scope.filteredBookings = [];
        $scope.selectedFilter = 'all';

        function parseNetDate(netDateStr) {
            if (!netDateStr) return null;
            var match = /\/Date\((\d+)\)\//.exec(netDateStr);
            if (match && match[1]) return new Date(parseInt(match[1], 10));
            return new Date(netDateStr);
        }

        function pad2(n) { return String(n).padStart(2, "0"); }

        function formatDateTime(d, startTime, endTime) {
            if (!d) return "N/A";
            var day = pad2(d.getDate());
            var mon = pad2(d.getMonth() + 1);
            var yr = d.getFullYear();
            var st = (startTime || "").substring(0, 5);
            var et = (endTime || "").substring(0, 5);
            return `${day}/${mon}/${yr} ${st}-${et}`;
        }

        function extractCancelReason(notes) {
            if (!notes) return "";
            var key = "Cancel reason:";
            var idx = notes.indexOf(key);
            if (idx === -1) return "";
            return notes.substring(idx + key.length).trim();
        }

        $scope.loadBookings = function () {
            return $http.get("/booking/List").then(function (resp) {
                var rows = resp.data || [];

                $scope.bookings = rows.map(function (b) {
                    var dateObj = parseNetDate(b.BookingDate);
                    var status = (b.Status || "Pending").trim();

                    var serviceLabel = "ServiceId #" + b.ServiceId;
                    if (b.Notes && b.Notes.indexOf("Services:") !== -1) {
                        var s = b.Notes.split("|")[0];
                        serviceLabel = s.replace("Services:", "").trim();
                    }

                    return {
                        bookingId: b.BookingId,
                        clientName: "Customer #" + b.CustomerId,
                        service: serviceLabel,
                        dateTime: formatDateTime(dateObj, b.StartTime, b.EndTime),
                        contact: b.Notes ? b.Notes : "N/A",
                        status: status,
                        cancelReason: extractCancelReason(b.Notes),
                        _raw: b
                    };
                });

                $scope.filterBookings($scope.selectedFilter);
            }).catch(function (err) {
                console.error("loadBookings error:", err);
                alert("❌ Failed to load bookings. Check console.");
            });
        };

        $scope.filterBookings = function (filter) {
            $scope.selectedFilter = filter;

            var today = new Date();
            today.setHours(0, 0, 0, 0);

            function parseCardDateTime(card) {
                if (!card || !card.dateTime || card.dateTime === "N/A") return null;
                var datePart = card.dateTime.split(" ")[0];
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
                $scope.filteredBookings = $scope.bookings;
            } else if (filter === 'today') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    var d = parseCardDateTime(b);
                    return d && d.getTime() === today.getTime();
                });
            } else if (filter === 'upcoming') {
                $scope.filteredBookings = $scope.bookings.filter(function (b) {
                    var d = parseCardDateTime(b);
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

        if (window.location.pathname.toLowerCase().indexOf("adminbookingpage") !== -1) {
            $scope.loadBookings();
        }

        $scope.updateBookingStatus = function (bookingId, newStatus) {
            var payload = { bookingId: bookingId, status: newStatus };

            return $http({
                method: "POST",
                url: "/Booking/UpdateStatus",
                data: $httpParamSerializerJQLike(payload),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {
                var card = $scope.bookings.find(b => b.bookingId === bookingId);
                if (card) card.status = newStatus;
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
                var card = $scope.bookings.find(b => b.bookingId === bookingId);
                if (card) {
                    card.status = "Cancelled";
                    card.cancelReason = reason;
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

        // ===== MISC UI =====

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