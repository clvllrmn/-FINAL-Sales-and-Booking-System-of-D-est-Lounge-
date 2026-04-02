var app = angular.module("DestLoungeSalesandBooking"); // ✅ add this
console.log("CONTROLLER FILE LOADED, app:", app);
var _currentContentType = null;


app.controller("DestLoungeSalesandBookingController",
    function ($scope, $window, DestLoungeSalesandBookingService, $http, $httpParamSerializerJQLike) {

        console.log("CONTROLLER REGISTERED");

        $scope.test = "Working";



    

    // ===== INITIALIZE DEFAULT VALUES =====
    $scope.bannerText = {
        hello: 'HELLO.',
        youre_looking: 'YOU\'RE LOOKING',
        gorgeous: 'gorgeous',
        today: 'TODAY.'
    };

    $scope.serviceInfo = {
        title: 'nails',
        tagline: 'Nailed it, every time.'
    };

    $scope.destInfo = {
        title: 'D\'est',
        text: 'is a modern beauty and relaxation space where self-care meets sophistication. A good place to stay where every visit is designed to be a luxurious and calming pause from everyday.'
    };

    $scope.polaroidImages = {
        1: '/Content/Pictures/nail1.jpg',
        2: '/Content/Pictures/nail2.jpg',
        3: '/Content/Pictures/nail3.jpg'
    };

    $scope.user = {};
    $scope.showPassword = false;
    $scope.showTerms = false;

    // ===== LOAD HOMEPAGE CONTENT FROM DATABASE =====
    $scope.loadHomepageContent = function () {
        console.log("loadHomepageContent called!");
        $http.get('/HomePageContent/GetAllContent')
            .then(function (response) {
                console.log("API Response:", response.data);
                if (response.data.success) {
                    var contentMap = {};
                    angular.forEach(response.data.data, function (item) {
                        contentMap[item.contentType] = item.contentValue;
                    });

                    if (contentMap['banner_hello']) $scope.bannerText.hello = contentMap['banner_hello'];
                    if (contentMap['banner_looking']) $scope.bannerText.youre_looking = contentMap['banner_looking'];
                    if (contentMap['banner_gorgeous']) $scope.bannerText.gorgeous = contentMap['banner_gorgeous'];
                    if (contentMap['banner_today']) $scope.bannerText.today = contentMap['banner_today'];
                    if (contentMap['service_title']) $scope.serviceInfo.title = contentMap['service_title'];
                    if (contentMap['service_tagline']) $scope.serviceInfo.tagline = contentMap['service_tagline'];
                    if (contentMap['dest_title']) $scope.destInfo.title = contentMap['dest_title'];
                    if (contentMap['dest_text']) $scope.destInfo.text = contentMap['dest_text'];

                    if (contentMap['polaroid_1']) $scope.polaroidImages[1] = contentMap['polaroid_1'] + '?t=' + Date.now();
                    if (contentMap['polaroid_2']) $scope.polaroidImages[2] = contentMap['polaroid_2'] + '?t=' + Date.now();
                    if (contentMap['polaroid_3']) $scope.polaroidImages[3] = contentMap['polaroid_3'] + '?t=' + Date.now();
                }
            })
            .catch(function (error) {
                console.error('Error loading homepage content:', error);
            });
    };

    // ===== EDIT HOMEPAGE TEXT =====
    $scope.editHomepageText = function (contentType, currentValue) {
        var newValue = prompt('Edit ' + contentType + ':', currentValue);
        if (newValue === null || newValue === "") return;

        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : "";

        $http({
            method: 'POST',
            url: '/HomePageContent/UpdateContent',
            data: $httpParamSerializerJQLike({
                contentType: contentType,
                contentValue: newValue,
                __RequestVerificationToken: tokenValue
            }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })
            .then(function (response) {
                if (response.data.success) {
                    alert('Updated!');
                    if (contentType === 'banner_hello') $scope.bannerText.hello = newValue;
                    if (contentType === 'banner_looking') $scope.bannerText.youre_looking = newValue;
                    if (contentType === 'banner_gorgeous') $scope.bannerText.gorgeous = newValue;
                    if (contentType === 'banner_today') $scope.bannerText.today = newValue;
                    if (contentType === 'service_title') $scope.serviceInfo.title = newValue;
                    if (contentType === 'service_tagline') $scope.serviceInfo.tagline = newValue;
                    if (contentType === 'dest_title') $scope.destInfo.title = newValue;
                    if (contentType === 'dest_text') $scope.destInfo.text = newValue;
                    $scope.loadHomepageContent();
                } else {
                    alert('Error: ' + response.data.message);
                }
            })
            .catch(function (error) {
                console.error('Request error:', error);
                alert('Failed to update');
            });
    };

    // ===== UPDATE POLAROID IMAGE =====
    $scope.updatePolaroidImage = function (slot) {
        var input = document.createElement('input');
        input.type = 'file';
        input.accept = 'image/*';

        input.onchange = function () {
            var file = input.files[0];
            if (!file) return;

            if (file.size > 2 * 1024 * 1024) {
                alert('Image is too large. Maximum size is 2MB.');
                return;
            }

            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';

            var formData = new FormData();
            formData.append('slot', slot);
            formData.append('imageFile', file);
            formData.append('__RequestVerificationToken', tokenValue);

            $http({
                method: 'POST',
                url: '/HomePageContent/UpdatePolaroidImage',
                data: formData,
                headers: { 'Content-Type': undefined }
            })
                .then(function (response) {
                    if (response.data.success) {
                        $scope.polaroidImages[slot] = response.data.imageUrl;
                        alert('Photo updated!');
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error updating polaroid image:', error);
                    alert('Failed to update photo.');
                });
        };

        input.click();
    };

    // ===== FAQ FUNCTIONS =====
    $scope.faqs = [];
    $scope.faqSearchText = '';

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
                }
            })
            .catch(function (error) {
                console.error('Error loading FAQs:', error);
            });
    };

    $scope.toggleFaq = function (index) {
        $scope.faqs[index].isOpen = !$scope.faqs[index].isOpen;
    };

        // FAQ Modal
        $scope.showFaqModal = false;
        $scope.faqEditingIndex = null;
        $scope.faqForm = { question: '', answer: '' };

        $scope.openFaqModal = function () {
            $scope.faqEditingIndex = null;
            $scope.faqForm = { question: '', answer: '' };
            $scope.showFaqModal = true;
        };

        $scope.openEditFaqModal = function (faq, index) {
            $scope.faqEditingIndex = index;
            $scope.faqForm = { question: faq.question, answer: faq.answer };
            $scope.showFaqModal = true;
        };

        $scope.closeFaqModal = function () {
            $scope.showFaqModal = false;
            $scope.faqForm = { question: '', answer: '' };
            $scope.faqEditingIndex = null;
        };

        $scope.closeFaqModalBackdrop = function ($event) {
            if ($event.target === $event.currentTarget) {
                $scope.closeFaqModal();
            }
        };

        $scope.saveFaqModal = function () {
            if (!$scope.faqForm.question.trim() || !$scope.faqForm.answer.trim()) return;

            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            var payload = {
                question: $scope.faqForm.question.trim(),
                answer: $scope.faqForm.answer.trim(),
                __RequestVerificationToken: tokenValue
            };

            if ($scope.faqEditingIndex !== null) {
                var faq = $scope.faqs[$scope.faqEditingIndex];
                $http({
                    method: 'POST',
                    url: '/FAQ/UpdateFAQ/' + faq.faqId,
                    data: $httpParamSerializerJQLike(payload),
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                }).then(function (response) {
                    if (response.data.success) {
                        $scope.loadFAQs();
                        $scope.showFaqModal = false;
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                });
            } else {
                $http({
                    method: 'POST',
                    url: '/FAQ/CreateFAQ',
                    data: $httpParamSerializerJQLike(payload),
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                }).then(function (response) {
                    if (response.data.success) {
                        $scope.loadFAQs();
                        $scope.showFaqModal = false;
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                });
            }
        };

    $scope.deleteFaq = function (index) {
        var faq = $scope.faqs[index];
        if (confirm('Are you sure you want to delete this FAQ?')) {
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            var payload = {
                __RequestVerificationToken: tokenValue
            };
            $http({
                method: 'POST',
                url: '/FAQ/DeleteFAQ/' + faq.faqId,
                data: $httpParamSerializerJQLike(payload),
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

    // ==== DELETED FAQS (TRASH) - FROM YOUR OLD CODE ====
    $scope.deletedFaqs = [];
    $scope.showFaqTrash = false;

    $scope.toggleFaqTrash = function () {
        $scope.showFaqTrash = !$scope.showFaqTrash;
        if ($scope.showFaqTrash) {
            $scope.loadDeletedFaqs();
        }
    };

    $scope.loadDeletedFaqs = function () {
        $http.get('/FAQ/GetDeletedFAQs')
            .then(function (response) {
                if (response.data.success) {
                    $scope.deletedFaqs = response.data.data;
                }
            })
            .catch(function (error) {
                console.error('Error loading deleted FAQs:', error);
            });
    };

    $scope.restoreFaq = function (faqId) {
        if (!confirm('Restore this FAQ?')) return;
        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : "";

        $http({
            method: 'POST',
            url: '/FAQ/RestoreFAQ/' + faqId,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).then(function (response) {
            if (response.data.success) {
                alert('FAQ restored successfully!');
                $scope.loadDeletedFaqs();
                $scope.loadFAQs();
            } else {
                alert('Error: ' + response.data.message);
            }
        }).catch(function (error) {
            console.error('Error restoring FAQ:', error);
            alert('Error restoring FAQ');
        });
    };

    $scope.permanentDeleteFaq = function (faqId) {
        if (!confirm('Permanently delete this FAQ? This CANNOT be undone.')) return;
        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : "";

        $http({
            method: 'POST',
            url: '/FAQ/PermanentDeleteFAQ/' + faqId,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).then(function (response) {
            if (response.data.success) {
                alert('FAQ permanently deleted.');
                $scope.loadDeletedFaqs();
            } else {
                alert('Error: ' + response.data.message);
            }
        }).catch(function (error) {
            console.error('Error permanently deleting FAQ:', error);
            alert('Error permanently deleting FAQ');
        });
    };

    // ===== SERVICES (DB-BACKED) =====
    $scope.services = [];
    $scope.showServiceModal = false;
    $scope.isEditMode = false;
    $scope.currentService = {};

    // Load services from the database
    $scope.loadServices = function () {
        $http.get('/Service/GetAllServices')
            .then(function (response) {
                if (response.data.success) {
                    $scope.services = response.data.data.map(function (s) {
                        return {
                            serviceId: s.serviceId,
                            name: s.name,
                            description: s.description,
                            price: s.price,
                            category: s.category || 'manicure',
                            image: s.image + '?t=' + Date.now()
                        };
                    });
                }
            })
            .catch(function (error) {
                console.error('Error loading services:', error);
            });
    };

    $scope.selectedCategory = 'all';

    $scope.filterServices = function (category) {
        $scope.selectedCategory = category;
    };

    $scope.categoryFilter = function (service) {
        if ($scope.selectedCategory === 'all') return true;
        return service.category === $scope.selectedCategory;
    };

    // Open modal to ADD a new service
    $scope.openAddServiceModal = function () {
        $scope.isEditMode = false;
        $scope.currentService = { name: '', description: '', price: null, category: '', image: '' };
        $scope.showServiceModal = true;
    };

    // Open modal to EDIT an existing service
    $scope.editService = function (service) {
        $scope.isEditMode = true;
        $scope.currentService = angular.copy(service);
        $scope.showServiceModal = true;
    };

    // Close the modal
    $scope.closeServiceModal = function () {
        $scope.showServiceModal = false;
        $scope.currentService = {};
    };

        $scope.handleImageUpload = function (file) {
            if (!file) return;

            // ✅ Block non-image files (videos, etc.)
            if (!file.type.startsWith('image/')) {
                alert('Only image files are allowed. Videos and other file types are not accepted.');
                document.getElementById('serviceImage').value = '';
                return;
            }

            // ✅ Updated to 5MB
            var maxSize = 5 * 1024 * 1024;
            if (file.size > maxSize) {
                alert('Image is too large. Maximum size is 5MB.');
                document.getElementById('serviceImage').value = '';
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                $scope.$apply(function () {
                    $scope.currentService.image = e.target.result;
                    $scope.currentService._imageFile = file;
                });
            };
            reader.readAsDataURL(file);
        };

    // Save service
        $scope.saveService = function (confirmDuplicate) {
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';

            var formData = new FormData();
            formData.append('name', $scope.currentService.name || '');
            formData.append('description', $scope.currentService.description || '');
            formData.append('price', $scope.currentService.price || 0);
            formData.append('category', $scope.currentService.category || '');
            formData.append('__RequestVerificationToken', tokenValue);

            // ✅ Pass confirmDuplicate flag
            formData.append('confirmDuplicate', confirmDuplicate ? 'true' : 'false');

            if ($scope.currentService._imageFile) {
                formData.append('imageFile', $scope.currentService._imageFile);
            }

            var url = $scope.isEditMode
                ? '/Service/UpdateService/' + $scope.currentService.serviceId
                : '/Service/CreateService';

            $http({
                method: 'POST',
                url: url,
                data: formData,
                headers: { 'Content-Type': undefined }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert($scope.isEditMode ? 'Service updated!' : 'Service created!');
                        $scope.closeServiceModal();
                        $scope.loadServices();

                        // ✅ Duplicate warning: ask admin to confirm
                    } else if (response.data.isDuplicate) {
                        if (confirm(response.data.message)) {
                            $scope.saveService(true); // re-submit with confirmDuplicate = true
                        }

                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error saving service:', error);
                    alert('Failed to save service.');
                });
        };

    // Soft-delete a service
    $scope.deleteService = function (service) {
        if (!confirm('Delete "' + service.name + '"? It can be restored later.')) return;

        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : '';

        $http({
            method: 'POST',
            url: '/Service/DeleteService/' + service.serviceId,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })
            .then(function (response) {
                if (response.data.success) {
                    alert('Service deleted.');
                    $scope.loadServices();
                } else {
                    alert('Error: ' + response.data.message);
                }
            })
            .catch(function (error) {
                console.error('Error deleting service:', error);
                alert('Failed to delete service.');
            });
    };

    // ===== DELETED SERVICES (TRASH) =====
    $scope.deletedServices = [];
    $scope.showServiceTrash = false;

    $scope.toggleServiceTrash = function () {
        $scope.showServiceTrash = !$scope.showServiceTrash;
        if ($scope.showServiceTrash) $scope.loadDeletedServices();
    };

    $scope.loadDeletedServices = function () {
        $http.get('/Service/GetDeletedServices')
            .then(function (response) {
                if (response.data.success) {
                    $scope.deletedServices = response.data.data;
                }
            })
            .catch(function (error) {
                console.error('Error loading deleted services:', error);
            });
    };

    $scope.restoreService = function (serviceId) {
        if (!confirm('Restore this service?')) return;

        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : '';

        $http({
            method: 'POST',
            url: '/Service/RestoreService/' + serviceId,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        })
            .then(function (response) {
                if (response.data.success) {
                    alert('Service restored!');
                    $scope.loadDeletedServices();
                    $scope.loadServices();
                } else {
                    alert('Error: ' + response.data.message);
                }
            })
            .catch(function (error) {
                console.error('Error restoring service:', error);
                alert('Error restoring service.');
            });
    };

    // ===== AUTO-LOAD on admin service page =====
    if (window.location.pathname.toLowerCase().indexOf('adminservicepage') !== -1) {
        $scope.loadServices();
    }

    // Also load on the public service page
    if (window.location.pathname.toLowerCase().indexOf('servicepage') !== -1) {
        $scope.loadServices();
    }

    // === BOOKING PAGE DATA ====
    $scope.nailTechs = [{ id: 1, name: "Name 1" }, { id: 2, name: "Name 2" }, { id: 3, name: "Name 3" }];

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
        nailTech: "",
        date: "",
        time: "",
        selectedServices: []
    };

    $scope.weekdayTimes = ["09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM", "07:00 PM"];
    $scope.weekendTimes = ["08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM", "08:00 PM"];
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

    $scope.calculateDownpayment = function () {
        return ($scope.calculateTotal() * 0.4).toFixed(2);
    };

        $scope.isBookingValid = function () {
            return $scope.booking.nailTech &&
                $scope.booking.date &&
                $scope.booking.time &&
                $scope.booking.selectedServices.length > 0 &&
                !$scope.dateFullyBooked;
        };

        $scope.getSelectedTechName = function () {
            if (!$scope.booking.nailTech) return '';
            var found = $scope.nailTechs.find(function (t) {
                return String(t.id) === String($scope.booking.nailTech);
            });
            return found ? found.name : '';
        };

    $scope.submitBooking = function () {
        console.log("window.loggedInUserId = ", window.loggedInUserId);
        if (!window.loggedInUserId || window.loggedInUserId === "null" || parseInt(window.loggedInUserId) <= 0) {
            alert("Please login first before booking.");
            window.location.href = "/Main/LoginPage";
            return;
        }
        if (!$scope.isBookingValid()) {
            alert("Please complete all booking fields.");
            return;
        }

        if (!$scope.calculateTotal() || $scope.calculateTotal() <= 0) {
            alert("Downpayment required");
            return;
        }

        

        var d = $scope.booking.date;
        var dateObj = (d instanceof Date) ? d : new Date(d);
        if (isNaN(dateObj.getTime())) {
            alert("Invalid date.");
            return;
        }
        var yyyy2 = dateObj.getFullYear();
        var mm2 = String(dateObj.getMonth() + 1).padStart(2, "0");
        var dd2 = String(dateObj.getDate()).padStart(2, "0");
        var bookingDate = yyyy2 + "-" + mm2 + "-" + dd2;

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
        var selectedServiceNames = ($scope.booking.selectedServices || []).map(function (s) {
            return s.name;
        });

        var payload = {
            customerId: parseInt(window.loggedInUserId),
            serviceId: serviceId,
            bookingDate: bookingDate,
            startTime: startTime,
            endTime: endTime,
            nailTech: String($scope.booking.nailTech || ""),
            downpayment: String($scope.calculateTotal() || ""),
            services: selectedServiceNames,  // ← ADD THIS LINE
            selectedServices: $scope.booking.selectedServices,  // ← OPTIONAL: keep original for backup
            notes: "Services: " + selectedServiceNames.join(", ")
        };
        var payload = {
            customerId: parseInt(window.loggedInUserId),
            serviceId: serviceId,
            bookingDate: bookingDate,
            startTime: startTime,
            endTime: endTime,
            nailTech: String($scope.booking.nailTech || ""),
            downpayment: String($scope.calculateTotal() || ""),
            notes: "Services: " + ($scope.booking.selectedServices || []).map(function (s) {
                return s.name;
            }).join(", ")
        };
        console.log("Booking payload:", payload);

        // 👉 CHECK SLOT AVAILABILITY BEFORE REDIRECT
        $http.get("/Booking/CheckSlot", {
            params: {
                date: bookingDate,
                startTime: startTime,
                nailTech: $scope.booking.nailTech
            }
        }).then(function (res) {
            if (res.data.taken) {
                alert("Slot already taken. Please choose another.");
                return;
            }

            // proceed to payment page
            sessionStorage.setItem("pendingBooking", JSON.stringify(payload));
            window.location.href = "/Main/PaymentPage";
        }).catch(function (error) {
            console.error("Error checking slot:", error);
            alert("Unable to check slot availability. Please try again.");
        });
        };

        



    // ===== SUBMIT PAYMENT (UPDATED WITH SERVICES) =====
    $scope.submitPayment = function () {
        console.log("CLICKED SUBMIT"); // 🔥 DEBUG

        var fileInput = document.getElementById("receiptUpload");

        if (!fileInput || fileInput.files.length === 0) {
            alert("Please upload proof of payment to proceed");
            return;
        }

        var file = fileInput.files[0];

        var booking = JSON.parse(sessionStorage.getItem("pendingBooking"));

        console.log("BOOKING DATA:", booking);

        // 🔥 ENSURE booking.services EXISTS (IMPORTANT)
        // This prevents the "join undefined" error
        if (!booking.services) {
            // If booking.selectedServices exists, map it to services
            if (booking.selectedServices && booking.selectedServices.length > 0) {
                booking.services = booking.selectedServices.map(function (s) {
                    return s.name;
                });
            }
            // If booking.notes contains services, extract them
            else if (booking.notes && booking.notes.includes("Services:")) {
                var servicesMatch = booking.notes.match(/Services:\s*(.+?)(?:\s*\||$)/);
                if (servicesMatch && servicesMatch[1]) {
                    booking.services = servicesMatch[1].split(',').map(function (s) {
                        return s.trim();
                    });
                } else {
                    booking.services = [];
                }
            }
            else {
                booking.services = [];
            }
        }

        var formData = new FormData();

        // 🔥 SAFE APPEND (avoid undefined)
        formData.append("customerId", booking.customerId || 0);
        formData.append("serviceId", booking.serviceId || 1);
        formData.append("bookingDate", booking.bookingDate);
        formData.append("startTime", booking.startTime);
        formData.append("endTime", booking.endTime);

        // 🔥 IMPORTANT: Append services as comma-separated string
        // This ensures we have booking.services available
        formData.append("services", booking.services.join(", "));
        formData.append("nailTech", booking.nailTech || "");
        formData.append("downpayment", booking.downpayment || "");

        // Also include the original selectedServices for backup (if exists)
        if (booking.selectedServices && booking.selectedServices.length > 0) {
            var servicesString = booking.selectedServices.map(function (s) {
                return s.name;
            }).join(", ");
            formData.append("selectedServices", servicesString);
        }

        formData.append("receipt", file);

        console.log("FORM DATA BEING SENT:");
        console.log("- services:", booking.services.join(", "));
        console.log("- nailTech:", booking.nailTech);
        console.log("- downpayment:", booking.downpayment);
        console.log("- receipt:", file.name);

        $http.post("/Booking/CreateWithReceipt", formData, {
            transformRequest: angular.identity,
            headers: { "Content-Type": undefined }
        }).then(function (res) {
            console.log("SERVER RESPONSE:", res);

            if (res.data && res.data.success) {
                alert("Booking successful!");
                sessionStorage.removeItem("pendingBooking");
                window.location.href = "/Main/CurrentBookingPage";
            } else {
                alert(res.data.message || "Booking failed");
            }
        }).catch(function (err) {
            console.error("ERROR:", err);
            alert("Server error");
        });
    };

    // ===== ADMIN BOOKING LIST =====
    $scope.bookings = [];
    $scope.filteredBookings = [];
    $scope.selectedFilter = 'all';

    function parseNetDate(netDateStr) {
        if (!netDateStr) return null;
        if (typeof netDateStr === "string") {
            var match = /\/Date\((\d+)\)\//.exec(netDateStr);
            if (match && match[1]) {
                return new Date(parseInt(match[1], 10));
            }
            var d = new Date(netDateStr);
            if (!isNaN(d.getTime())) return d;
        }
        if (netDateStr instanceof Date) return netDateStr;
        return null;
    }

    function pad2(n) {
        return String(n).padStart(2, "0");
    }

    function formatDateTime(d, startTime, endTime) {
        if (!d) return "N/A";
        var day = pad2(d.getDate());
        var mon = pad2(d.getMonth() + 1);
        var yr = d.getFullYear();
        var st = (startTime || "").substring(0, 5);
        var et = (endTime || "").substring(0, 5);
        return day + "/" + mon + "/" + yr + " " + st + "-" + et;
    }

    function extractCancelReason(notes) {
        if (!notes) return "";
        var key = "Cancel reason:";
        var idx = notes.indexOf(key);
        if (idx === -1) return "";
        return notes.substring(idx + key.length).trim();
    }

    $scope.loadBookings = function () {
        console.log("Calling /Booking/List ...");
        return $http.get("/Booking/List")
            .then(function (resp) {
                console.log("RAW /Booking/List response:", resp.data);
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
                        clientName: ((b.FirstName || "") + " " + (b.LastName || "")).trim() || ("Customer #" + b.CustomerId),
                        service: serviceLabel,
                        dateTime: formatDateTime(dateObj, b.StartTime, b.EndTime),
                        contact: b.Notes ? b.Notes : "N/A",
                        status: status,
                        cancelReason: extractCancelReason(b.Notes),
                        _raw: b
                    };
                });
                $scope.filterBookings($scope.selectedFilter);
            })
            .catch(function (err) {
                console.error("loadBookings error:", err);
                alert("Failed to load bookings. Check console.");
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
        } else if (filter === 'pending') {
            $scope.filteredBookings = $scope.bookings.filter(function (b) {
                return b.status === 'Pending';
            });
        }
        console.log("FILTERED bookings:", $scope.filteredBookings);
    };

    if (window.location.pathname.toLowerCase().indexOf("adminbookingpage") !== -1) {
        console.log("Admin booking page detected");
        $scope.selectedFilter = 'all';
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
            if (!resp.data.success) {
                alert(" " + resp.data.message);
                return;
            }
            var card = $scope.bookings.find(function (b) { return b.bookingId === bookingId; });
            if (card) card.status = newStatus;
            $scope.filterBookings($scope.selectedFilter);
            alert(resp.data.message || "Updated.");
            return resp.data;
        }).catch(function (err) {
            console.error(err);
            alert("X Update failed");
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
            alert(resp.data.message || "Cancelled!");
            $scope.loadUserBookings();
            var card = $scope.bookings.find(function (b) { return b.bookingId === bookingId; });
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
            alert("Cancel failed");
        });
    };

    // ===== SALES ANALYTICS =====
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
                $scope.sales = resp.data || 0;
                $scope.renderSalesChart($scope.sales.points || []);
            })
            .catch(function (err) {
                console.error("Sales analytics error:", err);
                alert("Failed to load Sales analytics. Check console.");
            });
    };

    $scope.renderSalesChart = function (points) {
        if (!window.Chart) {
            console.warn("Chart.js not found.");
            return;
        }
        var labels = (points || []).map(function (p) { return p.label; });
        var data = (points || []).map(function (p) { return Number(p.value || 0); });
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

    if (window.location.pathname.toLowerCase().indexOf("adminsalespage") !== -1) {
        setTimeout(function () {
            $scope.$applyAsync(function () {
                $scope.loadSalesAnalytics($scope.salesRange);
            });
        }, 0);
    }

    // ===== CONTACT DATA =====
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
                }
            })
            .catch(function (error) {
                console.error('Error loading contact info:', error);
            });
        };
        // Icon map — auto-assigned by type
        var iconMap = {
            phone: 'fa-solid fa-phone',
            email: 'fa-solid fa-envelope',
            address: 'fa-solid fa-location-dot',
            hours: 'fa-solid fa-clock'
        };

        var placeholderMap = {
            phone: '+63 912 345 6789',
            email: 'info@destlounge.com',
            address: '35 M. Cruz St, Marikina, Metro Manila',
            hours: 'Mon–Fri: 9AM–10PM'
        };

        var hintMap = {
            phone: 'The phone number customers can call',
            email: 'The email address customers can write to',
            address: 'Your full business address',
            hours: 'Your opening and closing times'
        };

        // Called when a type tile is clicked
        $scope.setContactType = function (type) {
            $scope.formData.infoType = type;
            $scope.formData.icon = iconMap[type] || 'fa-solid fa-circle-info';
            $scope.contactPlaceholder = placeholderMap[type] || 'Enter details...';
            $scope.contactHint = hintMap[type] || '';
        };

        // Update addNewContact to default to 'phone'
        $scope.addNewContact = function () {
            $scope.formData = { infoType: 'phone', icon: iconMap['phone'], label: '', value: '' };
            $scope.contactPlaceholder = placeholderMap['phone'];
            $scope.contactHint = hintMap['phone'];
            $scope.editingIndex = null;
            $scope.showModal = true;
        };

        // Update editContact to also set placeholder/hint
        $scope.editContact = function (index) {
            var c = $scope.contactInfo[index];
            $scope.formData = { infoType: c.infoType, icon: c.icon, label: c.label, value: c.value };
            $scope.contactPlaceholder = placeholderMap[c.infoType] || 'Enter details...';
            $scope.contactHint = hintMap[c.infoType] || '';
            $scope.editingIndex = index;
            $scope.showModal = true;
        };
        $scope.showModal = false;
        $scope.editingIndex = null;
        $scope.formData = {};

        $scope.addNewContact = function () {
            $scope.editingIndex = null;
            $scope.formData = {};
            $scope.showModal = true;
        };

        $scope.editContact = function (index) {
            $scope.editingIndex = index;
            $scope.formData = angular.copy($scope.contactInfo[index]);
            $scope.showModal = true;
        };

        $scope.closeModal = function () {
            $scope.showModal = false;
            $scope.formData = {};
            $scope.editingIndex = null;
        };

        $scope.closeModalBackdrop = function ($event) {
            if ($event.target === $event.currentTarget) {
                $scope.closeModal();
            }
        };

        $scope.saveContact = function () {
            if (!$scope.formData.label || !$scope.formData.value) return;

            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            var payload = {
                infoType: $scope.formData.infoType,
                label: $scope.formData.label,
                value: $scope.formData.value,
                icon: $scope.formData.icon || "",
                __RequestVerificationToken: tokenValue
            };

            if ($scope.editingIndex !== null) {
                var contactID = $scope.contactInfo[$scope.editingIndex].contactID;
                $http({
                    method: 'POST',
                    url: '/Contact/UpdateContact/' + contactID,
                    data: $httpParamSerializerJQLike(payload),
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                }).then(function (response) {
                    if (response.data.success) {
                        $scope.loadContactInfo();
                        $scope.closeModal();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                });
            } else {
                $http({
                    method: 'POST',
                    url: '/Contact/CreateContact',
                    data: $httpParamSerializerJQLike(payload),
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                }).then(function (response) {
                    if (response.data.success) {
                        $scope.loadContactInfo();
                        $scope.closeModal();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                });
            }
        };
    $scope.deleteContact = function (index) {
        var contact = $scope.contactInfo[index];
        if (confirm('Are you sure you want to delete this contact info?')) {
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';
            var payload = {
                __RequestVerificationToken: tokenValue
            };
            $http({
                method: 'POST',
                url: '/Contact/DeleteContact/' + contact.contactID,
                data: $httpParamSerializerJQLike(payload),
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

    // ===== DELETED CONTACTS (TRASH) =====
    $scope.deletedContacts = [];
    $scope.showContactTrash = false;

    $scope.toggleContactTrash = function () {
        $scope.showContactTrash = !$scope.showContactTrash;
        if ($scope.showContactTrash) {
            $scope.loadDeletedContacts();
        }
    };

    $scope.loadDeletedContacts = function () {
        $http.get('/Contact/GetDeletedContacts')
            .then(function (response) {
                if (response.data.success) {
                    $scope.deletedContacts = response.data.data;
                }
            })
            .catch(function (error) {
                console.error('Error loading deleted contacts:', error);
            });
    };

    $scope.restoreContact = function (contactID) {
        if (!confirm('Restore this contact info?')) return;
        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : '';

        $http({
            method: 'POST',
            url: '/Contact/RestoreContact/' + contactID,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).then(function (response) {
            if (response.data.success) {
                alert('Contact info restored successfully!');
                $scope.loadDeletedContacts();
                $scope.loadContactInfo();
            } else {
                alert('Error: ' + response.data.message);
            }
        }).catch(function (error) {
            console.error('Error restoring contact:', error);
            alert('Error restoring contact');
        });
    };

    $scope.permanentDeleteContact = function (contactID) {
        if (!confirm('Permanently delete this contact? This CANNOT be undone.')) return;
        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        var tokenValue = tokenElement ? tokenElement.value : '';

        $http({
            method: 'POST',
            url: '/Contact/PermanentDeleteContact/' + contactID,
            data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).then(function (response) {
            if (response.data.success) {
                alert('Contact permanently deleted.');
                $scope.loadDeletedContacts();
            } else {
                alert('Error: ' + response.data.message);
            }
        }).catch(function (error) {
            console.error('Error permanently deleting contact:', error);
            alert('Error permanently deleting contact');
        });
    };

    // ===== ON PAGE LOAD =====
    var currentPath = window.location.pathname.toLowerCase();
    console.log("Current path:", currentPath);

    if (currentPath.indexOf("admincontactpage") !== -1 || currentPath.indexOf("contactpage") !== -1) {
        $scope.loadContactInfo();
    }

    if (currentPath.indexOf("faqspage") !== -1) {
        $scope.loadFAQs();
    }

    if (currentPath === "/" || currentPath.indexOf("homepage") !== -1) {
        console.log("Homepage detected, loading content...");
        $scope.loadHomepageContent();
    } else {
        console.log("Not on homepage");
    }

        // ===== INBOX PAGE =====
        $scope.inboxItems = [];

        $scope.loadInboxItems = function () {
            $http.get('/Booking/GetInboxItems')
                .then(function (res) {
                    $scope.inboxItems = (res.data || []).map(function (b) {
                        var dateMatch = /\/Date\((\d+)\)\//.exec(b.bookingDate);
                        var d = dateMatch ? new Date(parseInt(dateMatch[1])) : new Date(b.bookingDate);
                        var dateStr = isNaN(d) ? 'N/A' : d.toLocaleDateString('en-PH', {
                            year: 'numeric', month: 'long', day: 'numeric'
                        });

                        // Format time helper
                        function fmt(t) {
                            if (!t) return '';
                            var parts = t.split(':');
                            var h = parseInt(parts[0]);
                            var m = parts[1] || '00';
                            var ampm = h >= 12 ? 'PM' : 'AM';
                            h = h % 12 || 12;
                            return h + ':' + m + ' ' + ampm;
                        }

                        // Get cancel reason from notes
                        var cancelReason = '';
                        if (b.notes && b.notes.indexOf('Cancel reason:') !== -1) {
                            var parts = b.notes.split('|');
                            for (var i = 0; i < parts.length; i++) {
                                if (parts[i].indexOf('Cancel reason:') !== -1)
                                    cancelReason = parts[i].replace('Cancel reason:', '').trim();
                            }
                        }

                        return {
                            bookingId: b.bookingId,
                            clientName: b.clientName,
                            service: b.service,
                            dateTime: dateStr + ' ' + fmt(b.startTime) + ' - ' + fmt(b.endTime),
                            status: b.status,
                            cancelReason: cancelReason,
                            notes: b.notes,
                            expanded: false
                        };
                    });
                })
                .catch(function (err) {
                    console.error('Error loading inbox:', err);
                });
        };

        if (window.location.pathname.toLowerCase().indexOf('admininboxpage') !== -1) {
            $scope.loadInboxItems();
        }

    // ===== UTILITY FUNCTIONS =====
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

    $scope.openEditModal = function (contentType, currentValue, label) {
        _currentContentType = contentType;
        document.getElementById('modalTitle').textContent = 'Edit: ' + label;
        document.getElementById('modalInput').value = currentValue || '';
        document.getElementById('editModal').style.display = 'flex';
        setTimeout(function () { document.getElementById('modalInput').focus(); }, 100);
    };

    // ===== USER BOOKINGS =====
        // ── Booking page helpers ──
        $scope.bookingsLoading = false;

        $scope.formatBookingDate = function (raw) {
            if (!raw) return '';
            var match = /\/Date\((\d+)\)\//.exec(raw);
            var d = match ? new Date(parseInt(match[1])) : new Date(raw);
            if (isNaN(d)) return raw;
            return d.toLocaleDateString('en-PH', { year: 'numeric', month: 'long', day: 'numeric' });
        };

        $scope.formatTime = function (t) {
            if (!t) return '';
            var parts = t.toString().split(':');
            var h = parseInt(parts[0]);
            var m = parts[1] || '00';
            var ampm = h >= 12 ? 'PM' : 'AM';
            h = h % 12 || 12;
            return h + ':' + m + ' ' + ampm;
        };

        $scope.getService = function (notes) {
            if (!notes) return 'N/A';
            var part = notes.split('|')[0];
            return part.replace('Services:', '').trim() || 'N/A';
        };

        $scope.getNailTech = function (notes) {
            if (!notes) return '';
            var parts = notes.split('|');
            for (var i = 0; i < parts.length; i++) {
                if (parts[i].indexOf('NailTech:') !== -1)
                    return parts[i].replace('NailTech:', '').trim();
            }
            return '';
        };

        $scope.getDownpayment = function (notes) {
            if (!notes) return '';
            var parts = notes.split('|');
            for (var i = 0; i < parts.length; i++) {
                if (parts[i].indexOf('Downpayment:') !== -1)
                    return parts[i].replace('Downpayment:', '').trim();
            }
            return '';
        };

        $scope.getCancelReason = function (notes) {
            if (!notes) return '';
            var parts = notes.split('|');
            for (var i = 0; i < parts.length; i++) {
                if (parts[i].indexOf('Cancel reason:') !== -1)
                    return parts[i].replace('Cancel reason:', '').trim();
            }
            return '';
        };

        $scope.canCancel = function (b) {
            if (!b.BookingDate || !b.StartTime) return false;
            var match = /\/Date\((\d+)\)\//.exec(b.BookingDate);
            var bookingDay = match ? new Date(parseInt(match[1])) : new Date(b.BookingDate);
            if (isNaN(bookingDay)) return false;
            var parts = (b.StartTime || '00:00:00').split(':');
            bookingDay.setHours(parseInt(parts[0] || 0), parseInt(parts[1] || 0), 0, 0);
            var cutoff = new Date();
            cutoff.setHours(cutoff.getHours() + 24);
            return bookingDay > cutoff;
        };

        // ── Updated loadUserBookings with loading flag ──
        $scope.loadUserBookings = function () {
            if (!window.loggedInUserId || window.loggedInUserId === "null" || parseInt(window.loggedInUserId) <= 0) {
                console.log("No user logged in");
                return;
            }

            var userId = parseInt(window.loggedInUserId);
            $scope.bookingsLoading = true;

            return $http.get("/Booking/GetUserBookings?userId=" + userId)
                .then(function (res) {
                    $scope.userBookings = res.data || [];
                    $scope.bookingsLoading = false;
                    console.log("Loaded bookings:", $scope.userBookings);
                })
                .catch(function (err) {
                    console.error("Error loading bookings:", err);
                    $scope.bookingsLoading = false;
                });
        };


    if (window.loggedInUserId && window.loggedInUserId !== "null") {
        $scope.loadUserBookings();
    }

    // ===== NOTIFICATIONS =====
    $scope.notifications = [];
    $scope.showNotif = false;

    $scope.loadNotifications = function () {
        var userId = window.loggedInUserId;
        if (!window.loggedInUserId) return;

        return $http.get("/Booking/GetNotifications?userId=" + window.loggedInUserId)
            .then(function (res) {
                $scope.notifications = res.data || [];
            });
    };

    $scope.toggleNotif = function () {
        $scope.showNotif = !$scope.showNotif;
    };

    $scope.goToBooking = function () {
        window.location.href = "/Main/CurrentBookingPage";
    };

    $scope.checkReviewNotification = function () {
        var reviewNotif = $scope.notifications.find(n =>
            n.Message.includes("completed")
        );

        if (reviewNotif) {
            setTimeout(function () {
                if (confirm("Your booking is completed! Write a review?")) {
                    window.location.href = "/Main/ReviewPage";
                }
            }, 500);
        }
    };

    if (window.location.pathname.toLowerCase().indexOf("profile") !== -1) {
        $scope.loadNotifications();
    }
    if (window.location.pathname.toLowerCase().indexOf("currentbookingpage") !== -1) {
        console.log("Current booking page detected");
        $scope.loadNotifications();
    }

    $scope.numbersOnly = function (event) {
        var charCode = event.which || event.keyCode;
        if (charCode < 48 || charCode > 57) {
            event.preventDefault();
        }
    };

    $scope.goToReview = function (bookingId) {
        window.location.href = "/Main/ReviewPage?bookingId=" + bookingId;
    };

        $scope.cancelUserBooking = function (bookingId) {
            if (!confirm("Are you sure you want to cancel this booking?")) return;
            var reason = prompt("Reason for cancellation (optional):") || "";

        $http.post("/Booking/Cancel",
            $httpParamSerializerJQLike({
                bookingId: bookingId,
                reason: reason
            }),
            { headers: { "Content-Type": "application/x-www-form-urlencoded" } }
        ).then(function () {
            alert("Booking cancelled");
            $scope.loadUserBookings();
        });
    };

    // ===== PAYMENT SETTINGS =====
    $scope.payment = {};
    $scope.paymentInfo = {};

    $scope.savePayment = function () {
        var file = document.getElementById("qrUpload").files[0];

        var formData = new FormData();
        formData.append("gcash", $scope.payment.gcash);
        formData.append("bank", $scope.payment.bank);
        formData.append("qr", file);

        $http.post("/Admin/SavePayment", formData, {
            headers: { "Content-Type": undefined }
        }).then(function () {
            alert("Saved!");
        });
    };

    $scope.loadPaymentInfo = function () {
        $http.get("/Admin/GetPaymentInfo")
            .then(function (res) {
                $scope.paymentInfo = res.data;
            });
    };

    // CALL ON PAGE LOAD
        $scope.loadPaymentInfo();

       

      

});

// ===== COMPARE DIRECTIVE =====
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