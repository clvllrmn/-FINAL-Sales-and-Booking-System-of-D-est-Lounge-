var app = angular.module("DestLoungeSalesandBooking");
console.log("CONTROLLER FILE LOADED, app:", app);
var _currentContentType = null;

app.controller("DestLoungeSalesandBookingController",
    function ($scope, $window, $http, $sce, $httpParamSerializerJQLike, $timeout) {

        console.log("CONTROLLER REGISTERED");

        $scope.test = "Working";
        $scope.myReviews = [];

        // ===== PAYMENT PAGE VARIABLES =====
        $scope.bookingSummary = null;
        $scope.paymentInfo = {};
        $scope.isLoading = false;
        $scope.loadError = null;
        $scope.isSubmitting = false;
        $scope.bookingReceipt = null;

        // Add these scope variables for image modal
        $scope.showImageModal = false;
        $scope.selectedImage = null;

        $scope.closeLightbox = function () {
            $scope.lightboxPhoto = null;
        };
        $scope.adminInboxCount = 0;

        $scope.loadAdminInboxCount = function () {
            $http.get('/Booking/GetInboxItems')
                .then(function (response) {
                    if (response.data && angular.isArray(response.data)) {
                        $scope.adminInboxCount = response.data.filter(function (item) {
                            return item.status === "Pending";
                        }).length;
                    } else {
                        $scope.adminInboxCount = 0;
                    }
                })
                .catch(function (error) {
                    console.error("Error loading inbox count:", error);
                    $scope.adminInboxCount = 0;
                });
        };

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
            1: '',
            2: '',
            3: ''
        };

        $scope.user = {};
        $scope.showPassword = false;
        $scope.showTerms = false;

        // ===== REVIEWS INITIALIZATION =====
        $scope.reviews = [];
        $scope.pagedReviews = [];

        $scope.openReviewLightbox = function (img) {
            $scope.lightboxPhoto = {
                imageUrl: img,
                caption: 'Review Image',
                description: ''
            };
        };

        // ===== REVIEW SETTINGS =====
        $scope.reviewsPage = 1;
        $scope.reviewsPerPage = 6;
        $scope.reviewStarFilter = '';
        $scope.reviewSearch = '';

        $scope.loadReviews = function () {
            var path = window.location.pathname.toLowerCase();
            var url = path.indexOf('admingallerypage') !== -1
                ? '/Main/GetAdminReviews'
                : '/Main/GetPublicReviews';

            $http.get(url)
                .then(function (res) {
                    console.log("REVIEWS DATA:", res.data);
                    $scope.reviews = (res.data || []).map(function (r) {
                        // Ensure images is always an array
                        var images = r.images || [];
                        if (typeof images === 'string') {
                            try {
                                images = JSON.parse(images);
                            } catch (e) {
                                images = [];
                            }
                        }
                        // Handle date properly
                        var reviewDate = r.date ? new Date(r.date) : (r.CreatedAt ? new Date(r.CreatedAt) : new Date());

                        return {
                            reviewId: r.ReviewId || r.reviewId,
                            rating: Number(r.Rating || r.rating || 0),
                            reviewText: r.ReviewText || r.reviewText || 'No comment provided.',
                            serviceAvailed: r.ServiceAvailed || r.serviceAvailed || "Nail Service",
                            date: reviewDate,
                            images: images,
                            flagged: r.Flagged || r.flagged || false,
                            isArchived: r.IsArchived || r.isArchived || false,
                            flagReason: r.FlagReason || r.flagReason || "",
                            flagNote: r.FlagNote || r.flagNote || ""
                        };
                    });
                    $scope.reviewsTotalPages = Math.ceil($scope.reviews.length / $scope.reviewsPerPage);
                    $scope.updatePagedReviews();
                })
                .catch(function (err) {
                    console.error("Error loading reviews:", err);
                });
        };

        $scope.showReviewForm = false;

        $scope.goToReview = function (bookingId) {
            window.location.href = "/Main/ReviewPage?bookingId=" + bookingId;
        };

        // ===== PAGINATION =====
        $scope.updatePagedReviews = function () {
            var filtered = ($scope.reviews || []).filter(function (review) {
                if ($scope.reviewStarFilter !== '' &&
                    Number(review.rating) !== Number($scope.reviewStarFilter)) {
                    return false;
                }
                if ($scope.reviewSearch) {
                    var q = $scope.reviewSearch.toLowerCase();
                    var textMatch = (review.reviewText || "").toLowerCase().includes(q);
                    var serviceMatch = (review.serviceAvailed || "").toLowerCase().includes(q);
                    var dateMatch = review.date
                        ? new Date(review.date).toLocaleDateString().toLowerCase().includes(q)
                        : false;
                    if (!textMatch && !serviceMatch && !dateMatch) {
                        return false;
                    }
                }
                return true;
            });
            $scope.reviewsTotalPages = Math.ceil(filtered.length / $scope.reviewsPerPage) || 1;
            if ($scope.reviewsPage > $scope.reviewsTotalPages) {
                $scope.reviewsPage = 1;
            }
            var start = ($scope.reviewsPage - 1) * $scope.reviewsPerPage;
            $scope.pagedReviews = filtered.slice(start, start + $scope.reviewsPerPage);
        };

        $scope.setReviewStarFilter = function (rating) {
            $scope.reviewStarFilter = rating;
            $scope.reviewsPage = 1;
            $scope.updatePagedReviews();
        };

        $scope.clearReviewStarFilter = function () {
            $scope.reviewStarFilter = '';
            $scope.reviewsPage = 1;
            $scope.updatePagedReviews();
        };

        $scope.customerReviewFilter = function (review) {
            if ($scope.reviewStarFilter && review.rating != $scope.reviewStarFilter) return false;
            if ($scope.reviewSearch && !review.reviewText.toLowerCase().includes($scope.reviewSearch.toLowerCase())) return false;
            return true;
        };

        $scope.reviewsNext = function () {
            if ($scope.reviewsPage < $scope.reviewsTotalPages) {
                $scope.reviewsPage++;
                $scope.updatePagedReviews();
            }
        };

        $scope.reviewsPrev = function () {
            if ($scope.reviewsPage > 1) {
                $scope.reviewsPage--;
                $scope.updatePagedReviews();
            }
        };

        $scope.averageRating = function () {
            if ($scope.reviews.length === 0) return 0;
            let total = $scope.reviews.reduce((sum, r) => sum + r.rating, 0);
            return (total / $scope.reviews.length).toFixed(1);
        };

        $scope.roundedAvg = function () {
            return Math.round($scope.averageRating());
        };


        $scope.openReviewLightbox = function (imgUrl) {
            console.log("Opening lightbox with image:", imgUrl);
            $scope.selectedImage = imgUrl;
            $scope.showImageModal = true;
            $scope.$applyAsync();
        };

        $scope.closeImageModal = function () {
            $scope.showImageModal = false;
            $scope.selectedImage = null;
        };


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

        $scope.deletedPolaroids = [];
        $scope.showPolaroidArchive = false;

        $scope.deletePolaroidImage = function (slot) {
            if (!confirm("Delete this homepage photo?")) return;
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';
            var formData = new FormData();
            formData.append('slot', slot);
            formData.append('__RequestVerificationToken', tokenValue);
            $http({
                method: 'POST',
                url: '/HomePageContent/DeletePolaroidImage',
                data: formData,
                headers: { 'Content-Type': undefined }
            })
                .then(function (response) {
                    if (response.data.success) {
                        $scope.polaroidImages[slot] = '';
                        alert('Photo deleted successfully!');
                        $scope.loadHomepageContent();
                        $scope.loadDeletedPolaroids();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error deleting polaroid image:', error);
                    alert('Failed to delete photo.');
                });
        };

        $scope.togglePolaroidArchive = function () {
            $scope.showPolaroidArchive = !$scope.showPolaroidArchive;
            if ($scope.showPolaroidArchive) {
                $scope.loadDeletedPolaroids();
            }
        };

        $scope.loadDeletedPolaroids = function () {
            $http.get('/HomePageContent/GetDeletedPolaroids')
                .then(function (response) {
                    if (response.data.success) {
                        $scope.deletedPolaroids = response.data.data;
                    }
                })
                .catch(function (error) {
                    console.error('Error loading deleted polaroids:', error);
                });
        };

        $scope.restorePolaroidImage = function (archiveId) {
            if (!confirm("Restore this photo?")) return;
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';
            $http({
                method: 'POST',
                url: '/HomePageContent/RestorePolaroidImage/' + archiveId,
                data: $.param({ __RequestVerificationToken: tokenValue }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        alert('Photo restored successfully!');
                        $scope.loadHomepageContent();
                        $scope.loadDeletedPolaroids();
                    } else {
                        alert('Error: ' + response.data.message);
                    }
                })
                .catch(function (error) {
                    console.error('Error restoring polaroid image:', error);
                    alert('Failed to restore photo.');
                });
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
                var payload = { __RequestVerificationToken: tokenValue };
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

        $scope.openAddServiceModal = function () {
            $scope.isEditMode = false;
            $scope.currentService = { name: '', description: '', price: null, category: '', image: '' };
            $scope.showServiceModal = true;
        };

        $scope.editService = function (service) {
            $scope.isEditMode = true;
            $scope.currentService = angular.copy(service);
            $scope.showServiceModal = true;
        };

        $scope.closeServiceModal = function () {
            $scope.showServiceModal = false;
            $scope.currentService = {};
        };

        $scope.handleImageUpload = function (file) {
            if (!file) return;
            if (!file.type.startsWith('image/')) {
                alert('Only image files are allowed.');
                var fileInput = document.getElementById('serviceImage');
                if (fileInput) fileInput.value = '';
                return;
            }
            var maxSize = 5 * 1024 * 1024;
            if (file.size > maxSize) {
                alert('Image is too large. Maximum size is 5MB.');
                var fileInput = document.getElementById('serviceImage');
                if (fileInput) fileInput.value = '';
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

        $scope.saveService = function (confirmDuplicate) {
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';
            var formData = new FormData();
            formData.append('name', $scope.currentService.name || '');
            formData.append('description', $scope.currentService.description || '');
            formData.append('price', $scope.currentService.price || 0);
            formData.append('category', $scope.currentService.category || '');
            formData.append('__RequestVerificationToken', tokenValue);
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
                    } else if (response.data.isDuplicate) {
                        if (confirm(response.data.message)) {
                            $scope.saveService(true);
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

        $scope.permanentDeleteService = function (serviceId) {
            if (!confirm('Permanently delete this service? This CANNOT be undone.')) return;
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : '';
            $http({
                method: 'POST',
                url: '/Service/PermanentDeleteService/' + serviceId,
                data: $httpParamSerializerJQLike({ __RequestVerificationToken: tokenValue }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (response) {
                if (response.data.success) {
                    alert('Service permanently deleted.');
                    $scope.loadDeletedServices();
                } else {
                    alert('Error: ' + response.data.message);
                }
            }).catch(function (error) {
                console.error('Error permanently deleting service:', error);
                alert('Error permanently deleting service.');
            });
        };

        if (window.location.pathname.toLowerCase().indexOf('admin') !== -1) {
            $scope.loadAdminInboxCount();
        }

        if (window.location.pathname.toLowerCase().indexOf('adminservicepage') !== -1) {
            $scope.loadServices();
        }

        if (window.location.pathname.toLowerCase().indexOf('servicepage') !== -1) {
            $scope.loadServices();
        }

        // === BOOKING PAGE DATA ====
        $scope.nailTechs = [];

        $scope.loadNailTechs = function () {
            $http.get('/Main/GetNailTechs')
                .then(function (response) {
                    $scope.nailTechs = (response.data || []).filter(function (t) {
                        return t.status !== 'inactive';
                    });
                })
                .catch(function (error) {
                    console.error('Error loading nail techs:', error);
                    $scope.nailTechs = [];
                });
        };

        $scope.loadBookingServices = function () {
            $http.get('/Service/GetAllServices')
                .then(function (response) {
                    if (response.data.success) {
                        $scope.bookingServices = response.data.data.map(function (s) {
                            return {
                                serviceId: s.serviceId,
                                name: s.name,
                                price: s.price,
                                selected: false
                            };
                        });
                    } else {
                        $scope.bookingServices = [];
                    }
                })
                .catch(function (error) {
                    console.error('Error loading booking services:', error);
                    $scope.bookingServices = [];
                });
        };

        if (window.location.pathname.toLowerCase().indexOf('bookingpage') !== -1) {
            console.log("Booking page detected");
            $scope.loadBookingServices();
            $scope.loadNailTechs();
        }

        $scope.booking = {
            nailTech: "",
            date: "",
            time: "",
            selectedServices: []
        };

        $scope.weekdayTimes = ["09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM", "07:00 PM"];
        $scope.weekendTimes = ["08:00 AM", "10:00 AM", "12:00 PM", "02:00 PM", "04:00 PM", "06:00 PM", "08:00 PM"];
        $scope.availableTimes = [];
        $scope.takenTimes = [];

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

        $scope.formatDateOnly = function (dateValue) {
            if (!dateValue) return "";
            var d;
            if (dateValue instanceof Date) {
                d = dateValue;
            } else {
                d = new Date(dateValue);
            }
            if (isNaN(d.getTime())) return "";
            var y = d.getFullYear();
            var m = String(d.getMonth() + 1).padStart(2, '0');
            var day = String(d.getDate()).padStart(2, '0');
            return y + '-' + m + '-' + day;
        };

        $scope.to12Hour = function (timeStr) {
            if (!timeStr) return "";
            var parts = timeStr.split(':');
            var h = parseInt(parts[0], 10);
            var m = parts[1] || "00";
            var suffix = h >= 12 ? "PM" : "AM";
            h = h % 12;
            if (h === 0) h = 12;
            return String(h).padStart(2, '0') + ":" + m + " " + suffix;
        };

        $scope.normalizeTime = function (time) {
            return (time || "").trim().toUpperCase();
        };

        $scope.isTimeDisabled = function (time) {
            var taken = $scope.takenTimes.indexOf($scope.normalizeTime(time)) !== -1;
            var tooSoon = $scope.isPast24HourCutoff($scope.booking.date, time);
            return taken || tooSoon;
        };

        $scope.isPast24HourCutoff = function (dateValue, timeValue) {
            if (!dateValue || !timeValue) return false;
            var selectedDate;
            if (dateValue instanceof Date) {
                selectedDate = new Date(dateValue);
            } else {
                selectedDate = new Date(dateValue);
            }
            if (isNaN(selectedDate.getTime())) return false;
            var timeText = (timeValue || "").toString().trim().toUpperCase();
            var match = timeText.match(/^(\d{1,2}):(\d{2})\s*(AM|PM)$/);
            if (!match) return false;
            var hour = parseInt(match[1], 10);
            var minute = parseInt(match[2], 10);
            var ampm = match[3];
            if (ampm === "PM" && hour < 12) hour += 12;
            if (ampm === "AM" && hour === 12) hour = 0;
            selectedDate.setHours(hour, minute, 0, 0);
            var cutoff = new Date();
            cutoff.setHours(cutoff.getHours() + 24);
            return selectedDate < cutoff;
        };

        $scope.checkAvailability = function () {
            $scope.takenTimes = [];
            $scope.dateFullyBooked = false;
            if (!$scope.booking.date) {
                $scope.availableTimes = [];
                $scope.booking.time = '';
                return;
            }
            var selectedDate;
            if ($scope.booking.date instanceof Date) {
                selectedDate = $scope.booking.date;
            } else {
                var parts = $scope.booking.date.split('-');
                selectedDate = new Date(parts[0], parts[1] - 1, parts[2]);
            }
            var dayOfWeek = selectedDate.getDay();
            if (dayOfWeek === 0 || dayOfWeek === 5 || dayOfWeek === 6) {
                $scope.availableTimes = angular.copy($scope.weekendTimes);
            } else {
                $scope.availableTimes = angular.copy($scope.weekdayTimes);
            }
            if (!$scope.booking.nailTech) {
                if ($scope.booking.time && $scope.availableTimes.indexOf($scope.booking.time) === -1) {
                    $scope.booking.time = '';
                }
                return;
            }
            var year = selectedDate.getFullYear();
            var month = String(selectedDate.getMonth() + 1).padStart(2, '0');
            var day = String(selectedDate.getDate()).padStart(2, '0');
            var selectedDateStr = year + '-' + month + '-' + day;
            $http.get('/Booking/GetTakenSlots', {
                params: {
                    date: selectedDateStr,
                    nailTech: $scope.booking.nailTech
                }
            }).then(function (res) {
                var data = res.data || {};
                if (!data.success) {
                    console.error("GetTakenSlots error:", data.message);
                    return;
                }
                $scope.takenTimes = (data.takenSlots || []).map(function (slot) {
                    return $scope.normalizeTime(slot.StartTime);
                });
                $scope.dateFullyBooked = $scope.availableTimes.length > 0 &&
                    $scope.availableTimes.every(function (time) {
                        return $scope.takenTimes.indexOf($scope.normalizeTime(time)) !== -1;
                    });
                if ($scope.booking.time && $scope.isTimeDisabled($scope.booking.time)) {
                    $scope.booking.time = '';
                }
            }).catch(function (err) {
                console.error("GetTakenSlots error:", err);
            });
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
            return $scope.booking.nailTech || '';
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
                services: selectedServiceNames,
                selectedServices: $scope.booking.selectedServices,
                notes: "Services: " + selectedServiceNames.join(", ")
            };

            console.log("Booking payload:", payload);

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
                sessionStorage.setItem("pendingBooking", JSON.stringify(payload));
                window.location.href = "/Main/PaymentPage";
            }).catch(function (error) {
                console.error("Error checking slot:", error);
                alert("Unable to check slot availability. Please try again.");
            });
        };

        // ===== PAYMENT PAGE FUNCTIONS =====

        $scope.formatTime = function (time) {
            if (!time) return "";
            time = time.toString().trim();
            if (/am|pm/i.test(time)) {
                return time.replace(/\s+/g, " ").trim();
            }
            var parts = time.split(":");
            var hours = parseInt(parts[0], 10);
            var minutes = parts[1];
            if (isNaN(hours) || minutes === undefined) return time;
            var ampm = hours >= 12 ? "PM" : "AM";
            hours = hours % 12;
            if (hours === 0) hours = 12;
            return hours + ":" + minutes + " " + ampm;
        };

        $scope.loadPaymentInfo = function () {
            $scope.isLoading = true;
            $http.get("/Booking/GetPaymentInfo")
                .then(function (res) {
                    console.log("Payment info response:", res.data);
                    if (res.data && res.data.success !== false) {
                        $scope.paymentInfo = res.data;
                    } else {
                        $scope.paymentInfo = {
                            gcashNumber: "09394091925",
                            gcash: "09394091925",
                            qr: "/Content/Pictures/gcash-qr.png"
                        };
                    }
                    $scope.isLoading = false;
                })
                .catch(function (err) {
                    console.error("Error loading payment info:", err);
                    $scope.paymentInfo = {
                        gcashNumber: "09394091925",
                        gcash: "09394091925",
                        qr: "/Content/Pictures/gcash-qr.png"
                    };
                    $scope.isLoading = false;
                });
        };

        $scope.loadPaymentSummary = function () {
            $scope.isLoading = true;
            $scope.loadError = null;

            var pendingBooking = sessionStorage.getItem('pendingBooking');

            if (pendingBooking) {
                try {
                    var bookingData = JSON.parse(pendingBooking);
                    console.log("Loaded from sessionStorage:", bookingData);

                    var total = 0;
                    var services = [];

                    if (bookingData.selectedServices && bookingData.selectedServices.length > 0) {
                        services = bookingData.selectedServices;
                        total = services.reduce(function (sum, s) { return sum + (s.price || 0); }, 0);
                    } else if (bookingData.services && bookingData.services.length > 0) {
                        services = bookingData.services.map(function (s) {
                            return typeof s === 'string' ? { name: s, price: 0 } : s;
                        });
                        total = services.reduce(function (sum, s) { return sum + (s.price || 0); }, 0);
                    } else if (bookingData.serviceName) {
                        services = [{ name: bookingData.serviceName, price: bookingData.price || 0 }];
                        total = bookingData.price || 0;
                    }

                    var downpayment = total * 0.4;

                    $scope.bookingSummary = {
                        serviceName: bookingData.serviceName || (services[0] ? services[0].name : 'Nail Service'),
                        price: bookingData.price || (services[0] ? services[0].price : 0),
                        total: total,
                        downpayment: downpayment,
                        nailTech: bookingData.nailTech || 'Michael Jackson',
                        date: bookingData.bookingDate ? new Date(bookingData.bookingDate) : new Date(),
                        time: bookingData.startTime ? $scope.formatTime(bookingData.startTime) : '11:00 AM',
                        services: services,
                        customerId: bookingData.customerId,
                        bookingDate: bookingData.bookingDate,
                        startTime: bookingData.startTime,
                        endTime: bookingData.endTime
                    };

                    $scope.isLoading = false;
                    return;
                } catch (e) {
                    console.error('Error parsing sessionStorage:', e);
                }
            }

            var urlParams = new URLSearchParams(window.location.search);
            var bookingId = urlParams.get('bookingId');

            if (bookingId && bookingId !== "null" && bookingId !== "") {
                $http.get('/Booking/GetPaymentSummary', {
                    params: { bookingId: bookingId }
                })
                    .then(function (response) {
                        console.log("Payment summary response:", response.data);
                        $scope.isLoading = false;
                        if (response.data && response.data.success !== false) {
                            $scope.bookingSummary = response.data;
                        } else if (response.data && response.data.bookingId) {
                            var data = response.data;
                            var total = data.total || data.amount || 0;
                            $scope.bookingSummary = {
                                serviceName: data.serviceName || 'Nail Service',
                                price: data.price || total,
                                total: total,
                                downpayment: data.downpayment || (total * 0.4),
                                nailTech: data.nailTech || 'Michael Jackson',
                                date: data.bookingDate ? new Date(data.bookingDate) : new Date(),
                                time: data.startTime || '11:00 AM',
                                services: data.services || []
                            };
                        } else {
                            $scope.loadError = 'No booking information found. Please select a service first.';
                            $scope.bookingSummary = null;
                        }
                    })
                    .catch(function (error) {
                        console.log("Error loading payment summary:", error);
                        $scope.isLoading = false;
                        $scope.loadError = 'Failed to load booking details. Please go back and try again.';
                        $scope.bookingSummary = null;
                    });
            } else {
                $scope.isLoading = false;
                $scope.loadError = 'No booking found. Please select a service first.';
                $scope.bookingSummary = null;
            }
        };

        function calculateEndTime(startTime) {
            if (!startTime) return "13:00:00";
            var hour = 0, minute = 0;
            var timeStr = startTime.toString();
            if (timeStr.includes(':')) {
                var parts = timeStr.split(':');
                hour = parseInt(parts[0], 10);
                minute = parseInt(parts[1], 10);
                if (timeStr.toUpperCase().includes('PM') && hour < 12) hour += 12;
                if (timeStr.toUpperCase().includes('AM') && hour === 12) hour = 0;
            }
            hour += 2;
            if (hour >= 24) hour -= 24;
            return String(hour).padStart(2, '0') + ":" + String(minute).padStart(2, '0') + ":00";
        }

        $scope.submitPayment = function () {
            if ($scope.isSubmitting) return;

            var fileInput = document.querySelector('#receiptUpload');
            if (!fileInput) {
                fileInput = document.querySelector('.upload-section input[type="file"]');
            }

            if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
                alert("Please upload proof of payment to proceed");
                return;
            }

            var file = fileInput.files[0];

            if (!file.type.startsWith('image/')) {
                alert('Please upload an image file (JPG, PNG, etc.)');
                return;
            }

            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                return;
            }

            var booking = null;
            var pendingBooking = sessionStorage.getItem("pendingBooking");

            if (pendingBooking) {
                try {
                    booking = JSON.parse(pendingBooking);
                } catch (e) {
                    console.error("Error parsing pendingBooking:", e);
                }
            }

            if (!booking && $scope.bookingSummary) {
                booking = {
                    customerId: $scope.bookingSummary.customerId || window.loggedInUserId,
                    serviceId: 1,
                    bookingDate: $scope.bookingSummary.bookingDate || ($scope.bookingSummary.date ? $scope.bookingSummary.date.toISOString().split('T')[0] : null),
                    startTime: $scope.bookingSummary.startTime || $scope.bookingSummary.time,
                    endTime: $scope.bookingSummary.endTime,
                    services: $scope.bookingSummary.services ? $scope.bookingSummary.services.map(function (s) { return s.name; }) : [$scope.bookingSummary.serviceName],
                    nailTech: $scope.bookingSummary.nailTech,
                    downpayment: $scope.bookingSummary.downpayment
                };
            }

            if (!booking) {
                alert("Booking session expired. Please make a new booking.");
                window.location.href = "/Main/BookingPage";
                return;
            }

            if (!booking.customerId || booking.customerId === 0) {
                booking.customerId = window.loggedInUserId;
            }

            if (!booking.customerId || booking.customerId === "null" || parseInt(booking.customerId) <= 0) {
                alert("Please login first before submitting payment.");
                window.location.href = "/Main/LoginPage";
                return;
            }

            $scope.isSubmitting = true;

            var formData = new FormData();
            formData.append("customerId", booking.customerId || 0);
            formData.append("serviceId", booking.serviceId || 1);
            formData.append("bookingDate", booking.bookingDate);
            formData.append("startTime", booking.startTime);
            formData.append("endTime", booking.endTime || calculateEndTime(booking.startTime));
            formData.append("services", Array.isArray(booking.services) ? booking.services.join(", ") : booking.services);
            formData.append("nailTech", booking.nailTech || "");
            formData.append("downpayment", booking.downpayment || $scope.bookingSummary.downpayment || 0);
            formData.append("receipt", file);

            $http.post("/Booking/CreateWithReceipt", formData, {
                transformRequest: angular.identity,
                headers: { "Content-Type": undefined }
            })
                .then(function (res) {
                    console.log("SERVER RESPONSE:", res.data);
                    $scope.isSubmitting = false;

                    if (res.data && res.data.success) {
                        var refNo = res.data.referenceNo || "N/A";
                        alert("Booking successful! Reference #: " + refNo);
                        sessionStorage.removeItem("pendingBooking");
                        window.location.href = "/Main/CurrentBookingPage";
                    } else {
                        alert("ERROR: " + (res.data.message || "Booking failed"));
                    }
                })
                .catch(function (err) {
                    console.error("ERROR:", err);
                    $scope.isSubmitting = false;
                    alert("Server error: " + (err.statusText || "Please try again"));
                });
        };

        // ===== ADMIN BOOKING LIST =====
        $scope.bookings = [];
        $scope.filteredBookings = [];
        $scope.selectedFilter = 'all';

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
                    if (rows.success === false) {
                        console.error("Failed to load bookings:", rows.message);
                        $scope.bookings = [];
                        $scope.filteredBookings = [];
                        return;
                    }
                    $scope.bookings = rows.map(function (b) {
                        return {
                            bookingId: b.bookingId,
                            referenceNo: b.referenceNo || "REF- " + b.bookingId,
                            customerId: b.customerId,
                            serviceId: b.serviceId,
                            bookingDate: b.bookingDate,
                            startTime: b.startTime,
                            endTime: b.endTime,
                            status: (b.status || "Pending").trim(),
                            notes: b.notes || "",
                            createdAt: b.createdAt,
                            firstName: b.firstName || "",
                            lastName: b.lastName || "",
                            email: b.email || "N/A",
                            contactNumber: b.contactNumber || "N/A",
                            address: b.address || "N/A",
                            totalBill: b.totalBill || 0,
                            downpayment: b.downpayment || 0,
                            receiptPath: b.receiptPath || "",
                            clientName: b.clientName || "N/A",
                            service: b.service || "N/A",
                            dateTime: b.dateTime || "N/A",
                            contact: b.contact || "N/A",
                            cancelReason: extractCancelReason(b.notes)
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
                var mm = parseInt(parts[0], 10);
                var dd = parseInt(parts[1], 10);
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
            } else {
                $scope.filteredBookings = $scope.bookings;
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
                    alert(resp.data.message || "Failed to update booking.");
                    return;
                }
                var card = $scope.bookings.find(function (b) { return b.bookingId === bookingId; });
                if (card) {
                    card.status = newStatus;
                    alert(resp.data.message || `Booking ${card.referenceNo} updated to ${newStatus}.`);
                } else {
                    alert(resp.data.message || "Updated.");
                }
                $scope.filterBookings($scope.selectedFilter);
            }).catch(function (err) {
                console.error("UpdateStatus error:", err);
                alert("Failed to update booking.");
            });
        };

        $scope.cancelBooking = function (bookingId) {
            var reason = prompt("Reason for cancellation (optional):") || "";
            return $http({
                method: "POST",
                url: "/Booking/AdminCancel",
                data: $httpParamSerializerJQLike({
                    bookingId: bookingId,
                    reason: reason
                }),
                headers: { "Content-Type": "application/x-www-form-urlencoded" }
            }).then(function (resp) {
                if (!resp.data.success) {
                    alert(resp.data.message || "Failed to cancel booking.");
                    return;
                }
                var card = $scope.bookings.find(function (b) { return b.bookingId === bookingId; });
                if (card) {
                    card.status = "Cancelled";
                    card.cancelReason = reason;
                    if (reason) {
                        if (!card.notes) card.notes = "";
                        if (card.notes.indexOf("Cancel reason:") === -1) {
                            card.notes += (card.notes ? " | " : "") + "Cancel reason: " + reason;
                        }
                    }
                }
                $scope.filterBookings($scope.selectedFilter);
                alert(resp.data.message || "Booking cancelled.");
            }).catch(function (err) {
                console.error("AdminCancel error:", err);
                alert("Failed to cancel booking.");
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
            return $http.get("/Main/Analytics", { params: { range: range } })
                .then(function (resp) {
                    $scope.sales = resp.data || {};
                    $scope.renderSalesChart(resp.data.points || []);
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
            var canvas = document.getElementById("salesChart");
            if (!canvas) return;
            const existingChart = Chart.getChart(canvas);
            if (existingChart) {
                existingChart.destroy();
            }
            var labels = (points || []).map(p => p.label);
            var data = (points || []).map(p => Number(p.value || 0));
            var ctx = canvas.getContext("2d");
            salesChartInstance = new Chart(ctx, {
                type: "line",
                data: {
                    labels: labels,
                    datasets: [{
                        label: "Total Revenue",
                        data: data,
                        borderColor: "rgb(75, 192, 192)",
                        backgroundColor: "rgba(75, 192, 192, 0.2)",
                        tension: 0.35,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
        };

        if (window.location.pathname.toLowerCase().indexOf("adminsalespage") !== -1) {
            if (!$scope._salesLoaded) {
                $scope._salesLoaded = true;
                setTimeout(function () {
                    $scope.$applyAsync(function () {
                        $scope.loadSalesAnalytics($scope.salesRange);
                    });
                }, 0);
            }
        }

        // ===== CONTACT DATA =====
        $scope.loadContactInfo = function () {
            var isAdmin = window.location.pathname.toLowerCase().indexOf('admin') !== -1;
            var url = isAdmin ? '/Contact/GetAllContact' : '/PublicContact/GetAllContact';
            $http.get(url)
                .then(function (response) {
                    if (response.data.success) {
                        $scope.contactInfo = response.data.data.map(function (contact) {
                            var rawVal = contact.value || '';
                            return {
                                contactID: contact.contactID,
                                infoType: contact.infoType,
                                label: contact.label,
                                rawValue: rawVal,
                                value: isAdmin
                                    ? rawVal
                                    : $sce.trustAsHtml(rawVal.replace(/\n/g, '<br>')),
                                icon: contact.icon
                            };
                        });
                    }
                })
                .catch(function (error) {
                    console.error('Error loading contact info:', error);
                });
        };

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

        $scope.setContactType = function (type) {
            $scope.formData.infoType = type;
            $scope.formData.icon = iconMap[type] || 'fa-solid fa-circle-info';
            $scope.contactPlaceholder = placeholderMap[type] || 'Enter details...';
            $scope.contactHint = hintMap[type] || '';
        };

        $scope.addNewContact = function () {
            $scope.formData = { infoType: 'phone', icon: iconMap['phone'], label: '', value: '' };
            $scope.contactPlaceholder = placeholderMap['phone'];
            $scope.contactHint = hintMap['phone'];
            $scope.editingIndex = null;
            $scope.showModal = true;
        };

        $scope.testClick = function (index) {
            console.log("TEST CLICK index:", index);
        };

        $scope.editContact = function (index) {
            var c = $scope.contactInfo[index];
            if (!c) return;
            $scope.formData = {
                infoType: String(c.infoType || ''),
                icon: String(c.icon || ''),
                label: String(c.label || ''),
                value: String(c.rawValue || '')
            };
            $scope.contactPlaceholder = placeholderMap[c.infoType] || 'Enter details...';
            $scope.contactHint = hintMap[c.infoType] || '';
            $scope.editingIndex = index;
            $scope.showModal = true;
        };

        $scope.showModal = false;
        $scope.editingIndex = null;
        $scope.formData = {};

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
            if (!$scope.formData.infoType || !$scope.formData.label || !$scope.formData.value) {
                alert("Please complete all fields.");
                return;
            }
            $scope.formData.label = ($scope.formData.label || "").trim();
            $scope.formData.value = ($scope.formData.value || "").trim();
            if ($scope.formData.infoType === "phone") {
                var digitsOnly = $scope.formData.value.replace(/\D/g, "");
                if (digitsOnly.length !== 11) {
                    alert("Contact number must be exactly 11 digits.");
                    return;
                }
                $scope.formData.value = digitsOnly;
            }
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
                        alert("Error: " + response.data.message);
                    }
                }).catch(function (error) {
                    console.error("UpdateContact error:", error);
                    alert("Error: Failed to update contact info.");
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
                        alert("Error: " + response.data.message);
                    }
                }).catch(function (error) {
                    console.error("CreateContact error:", error);
                    alert("Error: Failed to create contact info.");
                });
            }
        };

        $scope.deleteContact = function (index) {
            var contact = $scope.contactInfo[index];
            if (confirm('Are you sure you want to delete this contact info?')) {
                var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                var tokenValue = tokenElement ? tokenElement.value : '';
                var payload = { __RequestVerificationToken: tokenValue };
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
                        function fmt(t) {
                            if (!t) return '';
                            var parts = t.split(':');
                            var h = parseInt(parts[0]);
                            var m = parts[1] || '00';
                            var ampm = h >= 12 ? 'PM' : 'AM';
                            h = h % 12 || 12;
                            return h + ':' + m + ' ' + ampm;
                        }
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
                            referenceNo: b.referenceNo || "REF- " + b.bookingId,
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
        $scope.bookingsLoading = false;
        $scope.currentBookings = [];
        $scope.bookingHistory = [];

        $scope.formatBookingDate = function (raw) {
            if (!raw) return '';
            var d = new Date(raw);
            if (isNaN(d)) return raw;
            return d.toLocaleDateString('en-PH', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        };

        $scope.extractServices = function (notes) {
            if (!notes) return 'N/A';
            var match = notes.match(/Services:\s*([^|]+)/i);
            return match ? match[1].trim() : 'N/A';
        };

        $scope.extractDownpayment = function (notes) {
            if (!notes) return 'N/A';
            var match = notes.match(/Downpayment:\s*([^|]+)/i);
            return match ? match[1].trim() : 'N/A';
        };

        $scope.canCancelBookingByValues = function (bookingDate, startTime) {
            if (!bookingDate || !startTime) return false;
            var datePart = bookingDate;
            if (bookingDate.indexOf('T') !== -1) {
                datePart = bookingDate.split('T')[0];
            }
            var d = new Date(datePart);
            if (isNaN(d.getTime())) return false;
            var timeText = (startTime || "").toString().trim().toUpperCase();
            if (/^\d{2}:\d{2}:\d{2}$/.test(timeText)) {
                var parts24 = timeText.split(':');
                d.setHours(parseInt(parts24[0], 10), parseInt(parts24[1], 10), 0, 0);
            } else {
                var match = timeText.match(/^(\d{1,2}):(\d{2})\s*(AM|PM)$/);
                if (!match) return false;
                var hour = parseInt(match[1], 10);
                var minute = parseInt(match[2], 10);
                var ampm = match[3];
                if (ampm === "PM" && hour < 12) hour += 12;
                if (ampm === "AM" && hour === 12) hour = 0;
                d.setHours(hour, minute, 0, 0);
            }
            var now = new Date();
            var diffHours = (d.getTime() - now.getTime()) / (1000 * 60 * 60);
            return diffHours > 24;
        };

        $scope.loadUserBookings = function () {
            console.log("🔥 loadUserBookings triggered");
            $scope.bookingsLoading = true;
            return $http.get("/Booking/GetUserBookings")
                .then(function (res) {
                    console.log("DATA FROM SERVER:", res.data);
                    if (!res.data || !res.data.success) {
                        $scope.currentBookings = [];
                        $scope.bookingHistory = [];
                        $scope.bookingsLoading = false;
                        return;
                    }
                    var now = new Date();
                    var today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
                    var bookings = (res.data.data || []).map(function (b) {
                        var dateObj = new Date(b.BookingDate);
                        return {
                            bookingId: b.BookingId,
                            referenceNo: b.ReferenceNo || ("REF-" + b.BookingId),
                            bookingDate: b.BookingDate,
                            startTime: b.StartTime,
                            endTime: b.EndTime,
                            status: b.Status,
                            notes: b.Notes || "",
                            nailTech: b.NailTech || "",
                            totalBill: b.TotalBill || 0,
                            hasReview: b.HasReview || false,
                            dateObj: dateObj,
                            receiptUrl: (function (notes) {
                                if (!notes) return "";
                                var match = notes.match(/Receipt:\s*([^\|]+)/i);
                                return match ? match[1].trim() : "";
                            })(b.Notes)
                        };
                    });
                    $scope.myReviews = [];
                    $scope.reviewEditModal = {
                        show: false,
                        reviewId: 0,
                        rating: 0,
                        reviewText: ""
                    };
                    $scope.openEditMyReview = function (r) {
                        $scope.reviewEditModal = {
                            show: true,
                            reviewId: r.reviewId,
                            rating: r.rating,
                            reviewText: r.reviewText
                        };
                    };
                    $scope.closeEditMyReview = function () {
                        $scope.reviewEditModal = {
                            show: false,
                            reviewId: 0,
                            rating: 0,
                            reviewText: ""
                        };
                    };
                    $scope.updateMyReview = function () {
                        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                        var tokenValue = tokenElement ? tokenElement.value : "";
                        $http({
                            method: 'POST',
                            url: '/Booking/UpdateMyReview',
                            data: $httpParamSerializerJQLike({
                                reviewId: $scope.reviewEditModal.reviewId,
                                rating: $scope.reviewEditModal.rating,
                                reviewText: $scope.reviewEditModal.reviewText,
                                __RequestVerificationToken: tokenValue
                            }),
                            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                        }).then(function (res) {
                            if (res.data && res.data.success) {
                                alert("Review updated.");
                                $scope.closeEditMyReview();
                                $scope.loadMyReviews();
                            } else {
                                alert(res.data.message || "Failed to update review.");
                            }
                        });
                    };
                    $scope.deleteMyReview = function (reviewId) {
                        if (!confirm("Delete this review?")) return;
                        var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                        var tokenValue = tokenElement ? tokenElement.value : "";
                        $http({
                            method: 'POST',
                            url: '/Booking/DeleteMyReview',
                            data: $httpParamSerializerJQLike({
                                reviewId: reviewId,
                                __RequestVerificationToken: tokenValue
                            }),
                            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                        }).then(function (res) {
                            if (res.data && res.data.success) {
                                alert("Review deleted.");
                                $scope.loadMyReviews();
                                $scope.loadUserBookings();
                            } else {
                                alert(res.data.message || "Failed to delete review.");
                            }
                        });
                    };
                    if (window.location.pathname.toLowerCase().includes("reviewpage")) {
                        $scope.loadMyReviews();
                    }
                    $scope.currentBookings = bookings.filter(function (b) {
                        return (
                            (b.status === "Pending" || b.status === "Approved") &&
                            b.dateObj >= today
                        );
                    });
                    $scope.bookingHistory = bookings.filter(function (b) {
                        return (
                            b.status === "Completed" ||
                            b.status === "Cancelled" ||
                            b.dateObj < today
                        );
                    });
                    console.log("CURRENT:", $scope.currentBookings);
                    console.log("HISTORY:", $scope.bookingHistory);
                    $scope.bookingsLoading = false;
                })
                .catch(function (err) {
                    console.error("LOAD USER BOOKINGS ERROR:", err);
                    $scope.currentBookings = [];
                    $scope.bookingHistory = [];
                    $scope.bookingsLoading = false;
                });
        };

        $scope.loadMyReviews = function () {
            return $http.get('/Main/GetMyReviews')
                .then(function (res) {
                    console.log("GetMyReviews response:", res.data);
                    if (!res.data || !res.data.success) {
                        $scope.myReviews = [];
                        return;
                    }
                    $scope.myReviews = (res.data.data || []).map(function (r) {
                        return {
                            reviewId: r.ReviewId,
                            bookingId: r.BookingId,
                            rating: r.Rating,
                            reviewText: r.ReviewText || "",
                            createdAt: r.CreatedAt ? new Date(parseInt(r.CreatedAt.substr(6))) : null,
                            flagged: r.Flagged || false,
                            flagReason: r.FlagReason || "",
                            flagNote: r.FlagNote || "",
                            images: r.Images || [],
                            booking: r.Booking || {}
                        };
                    });
                })
                .catch(function (err) {
                    console.error("loadMyReviews error:", err);
                    $scope.myReviews = [];
                });
        };

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

        $scope.toggleNotif = function ($event) {
            if ($event) $event.stopPropagation();
            $scope.showNotif = !$scope.showNotif;
        };

        $scope.goToBooking = function () {
            window.location.href = "/Main/CurrentBookingPage";
        };

        $scope.checkReviewNotification = function () {
            var reviewNotif = $scope.notifications.find(function (n) {
                return n.Message.includes("completed");
            });
            if (reviewNotif) {
                setTimeout(function () {
                    if (confirm("Your booking is completed! Write a review?")) {
                        window.location.href = "/Main/ReviewPage";
                    }
                }, 500);
            }
        };

        if (window.loggedInUserId) {
            $scope.loadNotifications();
        }

        if (window.location.pathname.toLowerCase().includes("currentbookingpage")) {
            console.log("Current booking page detected");
            $scope.loadUserBookings();
        }

        $scope.numbersOnly = function (event) {
            var charCode = event.which || event.keyCode;
            if (charCode < 48 || charCode > 57) {
                event.preventDefault();
            }
        };

        $scope.deleteNotification = function (notifId, $event) {
            if ($event) $event.stopPropagation();
            console.log("deleteNotification called, id =", notifId);
            $http({
                method: 'POST',
                url: '/Booking/DeleteNotification',
                data: 'notificationId=' + notifId,
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                console.log("response =", res.data);
                if (res.data.success) {
                    $scope.notifications = $scope.notifications.filter(function (n) {
                        return (n.NotificationId || n.notificationId) !== notifId;
                    });
                }
            }).catch(function (err) {
                console.log("error =", err);
            });
        };

        $scope.clearAllNotifications = function ($event) {
            if ($event) $event.stopPropagation();
            $http({
                method: 'POST',
                url: '/Booking/DeleteAllNotifications',
                data: '',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.notifications = [];
                    $scope.showNotif = false;
                }
            });
        };

        $scope.getUnreadCount = function () {
            return $scope.notifications.filter(function (n) {
                return !(n.IsRead || n.isRead);
            }).length;
        };

        $scope.markAsReadAndGo = function (notif, $event) {
            if ($event) $event.stopPropagation();
            var notifId = notif.NotificationId || notif.notificationId;
            var bookingId = notif.BookingId || notif.bookingId || null;
            if (!(notif.IsRead || notif.isRead)) {
                $http({
                    method: 'POST',
                    url: '/Booking/MarkNotificationRead',
                    data: 'notificationId=' + notifId,
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
                }).then(function () {
                    notif.IsRead = true;
                    notif.isRead = true;
                    if (bookingId) {
                        window.location.href = "/Main/CurrentBookingPage#booking-" + bookingId;
                    } else {
                        window.location.href = "/Main/CurrentBookingPage";
                    }
                }).catch(function () {
                    window.location.href = "/Main/CurrentBookingPage";
                });
            } else {
                if (bookingId) {
                    window.location.href = "/Main/CurrentBookingPage#booking-" + bookingId;
                } else {
                    window.location.href = "/Main/CurrentBookingPage";
                }
            }
        };

        $scope.verifyOtp = function () {
            if (!$scope.resetPassword.otp || $scope.resetPassword.otp.length !== 6) {
                alert("Please enter the 6-digit OTP.");
                return;
            }
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            $http({
                method: 'POST',
                url: '/Account/VerifyForgotPasswordOtp',
                data: $httpParamSerializerJQLike({
                    email: $scope.resetPassword.email,
                    otp: $scope.resetPassword.otp,
                    __RequestVerificationToken: tokenValue
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            })
                .then(function (response) {
                    if (response.data.success) {
                        $scope.forgotStep = 3;
                    } else {
                        alert(response.data.message || "Invalid OTP.");
                    }
                })
                .catch(function (err) {
                    console.error("VerifyOtp error:", err);
                    alert("Something went wrong. Please try again.");
                });
        };

        document.addEventListener('click', function () {
            $scope.$apply(function () {
                $scope.showNotif = false;
            });
        });

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

        // ── GALLERY (admin tab) ──────────────────────────────────────────────────
        $scope.galleryPhotos = [];
        $scope.adminTab = 'gallery';

        $scope.newPhoto = { file: null, caption: '', description: '', previewUrl: '' };

        $scope.onFileSelected = function (input) {
            var file = input.files[0];
            if (!file) return;
            var allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
            if (!allowed.includes(file.type)) {
                alert('Only JPG, PNG, WEBP, or GIF images are allowed.');
                input.value = '';
                return;
            }
            if (file.size > 5 * 1024 * 1024) {
                alert('File exceeds the 5 MB limit.');
                input.value = '';
                return;
            }
            $timeout(function () {
                $scope.newPhoto.file = file;
                $scope.newPhoto.previewUrl = URL.createObjectURL(file);
            });
        };

        $scope.loadGalleryPhotos = function () {
            $http.get('/Gallery/GetGalleryPhotos')
                .then(function (res) {
                    if (res.data.success) {
                        $scope.galleryPhotos = res.data.data.map(function (p) {
                            return {
                                galleryId: p.galleryId,
                                caption: p.caption,
                                description: p.description,
                                imageUrl: p.imageUrl,
                                editing: false,
                                editCaption: p.caption
                            };
                        });
                    }
                })
                .catch(function (err) { console.error('loadGalleryPhotos error:', err); });
        };

        $scope.uploadPhoto = function () {
            if (!$scope.newPhoto.file || !$scope.newPhoto.caption) return;
            var token = (document.querySelector('input[name="__RequestVerificationToken"]') || {}).value || '';
            var fd = new FormData();
            fd.append('caption', $scope.newPhoto.caption.trim());
            fd.append('description', $scope.newPhoto.description || '');
            fd.append('imageFile', $scope.newPhoto.file);
            fd.append('__RequestVerificationToken', token);
            $http.post('/Gallery/UploadPhoto', fd, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.galleryPhotos.unshift({
                        galleryId: res.data.galleryId,
                        caption: res.data.caption,
                        description: $scope.newPhoto.description || '',
                        imageUrl: res.data.imageUrl,
                        editing: false,
                        editCaption: res.data.caption
                    });
                    $scope.newPhoto = { file: null, caption: '', previewUrl: '' };
                    document.getElementById('photoFileInput').value = '';
                    $scope.showToast('Photo added successfully!', 'toast-success');
                } else {
                    $scope.showToast(res.data.message, 'toast-error');
                }
            }).catch(function () { $scope.showToast('Upload failed.', 'toast-error'); });
        };

        $scope.editModal = { show: false, photo: {}, caption: '', description: '', newImageFile: null, newImagePreview: '' };

        $scope.startEditPhoto = function (photo) {
            $scope.editModal = {
                show: true, photo: photo,
                caption: photo.caption,
                description: photo.description || '',
                newImageFile: null,
                newImagePreview: ''
            };
        };

        $scope.cancelEditPhoto = function () {
            $scope.editModal.show = false;
        };

        $scope.onEditImageSelected = function (input) {
            var file = input.files[0];
            if (!file) return;
            var allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
            if (!allowed.includes(file.type)) {
                alert('Only JPG, PNG, WEBP, or GIF images are allowed.');
                input.value = '';
                return;
            }
            if (file.size > 5 * 1024 * 1024) {
                alert('File exceeds the 5 MB limit.');
                input.value = '';
                return;
            }
            $timeout(function () {
                $scope.editModal.newImageFile = file;
                $scope.editModal.newImagePreview = URL.createObjectURL(file);
            });
        };

        $scope.saveEditPhoto = function (photo) {
            var token = (document.querySelector('input[name="__RequestVerificationToken"]') || {}).value || '';
            var fd = new FormData();
            fd.append('galleryId', photo.galleryId);
            fd.append('caption', $scope.editModal.caption);
            fd.append('description', $scope.editModal.description || '');
            fd.append('__RequestVerificationToken', token);
            if ($scope.editModal.newImageFile) {
                fd.append('newImageFile', $scope.editModal.newImageFile);
            }
            $http.post('/Gallery/UpdateCaption', fd, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            }).then(function (res) {
                if (res.data.success) {
                    photo.caption = $scope.editModal.caption;
                    photo.description = $scope.editModal.description;
                    if (res.data.imageUrl) photo.imageUrl = res.data.imageUrl;
                    $scope.editModal.show = false;
                    $scope.editModal.newImageFile = null;
                    $scope.editModal.newImagePreview = '';
                    $scope.showToast('Photo updated.', 'toast-success');
                } else {
                    $scope.showToast(res.data.message, 'toast-error');
                }
            });
        };

        $scope.deleteModal = { show: false, photo: {} };

        $scope.confirmDeletePhoto = function (photo) {
            $scope.deleteModal = { show: true, photo: photo };
        };

        $scope.deletePhoto = function () {
            var photo = $scope.deleteModal.photo;
            var token = (document.querySelector('input[name="__RequestVerificationToken"]') || {}).value || '';
            $http({
                method: 'POST',
                url: '/Gallery/DeletePhoto',
                data: $httpParamSerializerJQLike({
                    galleryId: photo.galleryId,
                    __RequestVerificationToken: token
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.galleryPhotos = $scope.galleryPhotos.filter(function (p) {
                        return p.galleryId !== photo.galleryId;
                    });
                    $scope.deleteModal.show = false;
                    $scope.showToast('Photo deleted.', 'toast-success');
                } else {
                    $scope.showToast(res.data.message, 'toast-error');
                }
            });
        };

        $scope.archivedGalleryPhotos = [];

        $scope.loadArchivedGallery = function () {
            $http.get('/Gallery/GetArchivedPhotos')
                .then(function (res) {
                    if (res.data.success) {
                        $scope.archivedGalleryPhotos = res.data.data.map(function (p) {
                            return {
                                galleryId: p.galleryId,
                                caption: p.caption,
                                description: p.description,
                                imageUrl: p.imageUrl,
                                archivedAt: p.archivedAt
                            };
                        });
                    }
                })
                .catch(function (err) { console.error('loadArchivedGallery error:', err); });
        };

        $scope.restoreGalleryPhoto = function (photo) {
            var token = (document.querySelector('input[name="__RequestVerificationToken"]') || {}).value || '';
            $http({
                method: 'POST', url: '/Gallery/RestorePhoto',
                data: $httpParamSerializerJQLike({ galleryId: photo.galleryId, __RequestVerificationToken: token }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.archivedGalleryPhotos = $scope.archivedGalleryPhotos.filter(function (p) {
                        return p.galleryId !== photo.galleryId;
                    });
                    $scope.loadGalleryPhotos();
                    $scope.showToast('Photo restored.', 'toast-success');
                } else { $scope.showToast(res.data.message, 'toast-error'); }
            });
        };

        $scope.permanentDeleteModal = { show: false, photo: {} };

        $scope.confirmPermanentDelete = function (photo) {
            $scope.permanentDeleteModal = { show: true, photo: photo };
        };

        $scope.permanentDeletePhoto = function () {
            var photo = $scope.permanentDeleteModal.photo;
            var token = (document.querySelector('input[name="__RequestVerificationToken"]') || {}).value || '';
            $http({
                method: 'POST', url: '/Gallery/PermanentDeletePhoto',
                data: $httpParamSerializerJQLike({ galleryId: photo.galleryId, __RequestVerificationToken: token }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.archivedGalleryPhotos = $scope.archivedGalleryPhotos.filter(function (p) {
                        return p.galleryId !== photo.galleryId;
                    });
                    $scope.permanentDeleteModal.show = false;
                    $scope.showToast('Photo permanently deleted.', 'toast-success');
                } else { $scope.showToast(res.data.message, 'toast-error'); }
            });
        };

        $scope.toast = { show: false, message: '', type: '' };

        $scope.showToast = function (message, type) {
            $scope.toast = { show: true, message: message, type: type || 'toast-success' };
            setTimeout(function () {
                $scope.$apply(function () { $scope.toast.show = false; });
            }, 3000);
        };

        $scope.galleryPhotosAll = [];
        $scope.pagedGallery = [];
        $scope.galleryPage = 1;
        $scope.galleryPerPage = 9;
        $scope.galleryTotalPages = 1;

        $scope.loadPublicGallery = function () {
            $http.get('/Gallery/GetGalleryPhotos')
                .then(function (res) {
                    if (res.data.success) {
                        $scope.galleryPhotosAll = res.data.data;
                        $scope.galleryTotalPages = Math.ceil(
                            $scope.galleryPhotosAll.length / $scope.galleryPerPage);
                        $scope.updatePagedGallery();
                    }
                });
        };

        $scope.updatePagedGallery = function () {
            var start = ($scope.galleryPage - 1) * $scope.galleryPerPage;
            $scope.pagedGallery = $scope.galleryPhotosAll.slice(start, start + $scope.galleryPerPage);
        };

        $scope.galleryNext = function () {
            if ($scope.galleryPage < $scope.galleryTotalPages) {
                $scope.galleryPage++;
                $scope.updatePagedGallery();
            }
        };

        $scope.galleryPrev = function () {
            if ($scope.galleryPage > 1) {
                $scope.galleryPage--;
                $scope.updatePagedGallery();
            }
        };

        $scope.reviewSearchFilter = function (review) {
            if ($scope.reviewStarFilter && String(review.rating) !== String($scope.reviewStarFilter)) {
                return false;
            }
            if ($scope.reviewSearch) {
                var q = $scope.reviewSearch.toLowerCase();
                var textMatch = (review.reviewText || "").toLowerCase().includes(q);
                var serviceMatch = (review.serviceAvailed || "").toLowerCase().includes(q);
                var dateMatch = review.date ? new Date(review.date).toLocaleDateString().toLowerCase().includes(q) : false;
                if (!textMatch && !serviceMatch && !dateMatch) {
                    return false;
                }
            }
            return true;
        };

        $scope.openLightbox = function (photo) {
            $scope.lightboxPhoto = photo;
        };

        $scope.closeLightbox = function () {
            $scope.lightboxPhoto = null;
        };

        $scope.openReviewLightbox = function (imgUrl) {
            $scope.selectedImage = imgUrl;
            $scope.showImageModal = true;
        };

        $scope.closeImageModal = function () {
            $scope.showImageModal = false;
            $scope.selectedImage = null;
        };

        $scope.flagModal = {
            show: false,
            review: null,
            reason: '',
            note: ''
        };

        $scope.openFlagModal = function (review) {
            $scope.flagModal.show = true;
            $scope.flagModal.review = review;
            $scope.flagModal.reason = '';
            $scope.flagModal.note = '';
        };

        $scope.closeFlagModal = function () {
            $scope.flagModal.show = false;
            $scope.flagModal.review = null;
            $scope.flagModal.reason = '';
            $scope.flagModal.note = '';
        };

        $scope.submitFlagReview = function () {
            if (!$scope.flagModal.review || !$scope.flagModal.reason) {
                alert("Please select a reason first.");
                return;
            }
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            $http({
                method: 'POST',
                url: '/Main/FlagReview',
                data: $httpParamSerializerJQLike({
                    reviewId: $scope.flagModal.review.reviewId,
                    reason: $scope.flagModal.reason,
                    note: $scope.flagModal.note || '',
                    __RequestVerificationToken: tokenValue
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
            }).then(function (res) {
                if (res.data && res.data.success) {
                    alert("Review flagged successfully.");
                    $scope.flagModal.review.flagged = true;
                    $scope.closeFlagModal();
                    $scope.loadReviews();
                } else {
                    alert((res.data && res.data.message) || "Failed to flag review.");
                }
            }).catch(function (err) {
                console.error("submitFlagReview error:", err);
                alert("Failed to flag review.");
            });
        };

        $scope.archiveReview = function (reviewId) {
            if (!confirm("Archive this review?")) return;
            $http({
                method: 'POST',
                url: '/Main/ArchiveReview',
                data: $httpParamSerializerJQLike({ reviewId: reviewId }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.reviews = $scope.reviews.filter(function (r) {
                        return r.reviewId !== reviewId;
                    });
                    $scope.loadArchivedReviews();
                    $scope.updatePagedReviews();
                } else {
                    alert("Archive failed.");
                }
            }).catch(function (err) {
                console.error("archiveReview error:", err);
                alert("Archive failed.");
            });
        };

        $scope.unflagReview = function (review) {
            if (!review || !review.reviewId) {
                alert("Invalid review.");
                return;
            }
            if (!confirm("Unflag this review?")) return;
            var tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenElement ? tokenElement.value : "";
            $http({
                method: 'POST',
                url: '/Main/UnflagReview',
                data: $httpParamSerializerJQLike({
                    reviewId: review.reviewId,
                    __RequestVerificationToken: tokenValue
                }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' }
            }).then(function (res) {
                if (res.data && res.data.success) {
                    review.flagged = false;
                    review.flagReason = "";
                    review.flagNote = "";
                    $scope.loadReviews();
                } else {
                    alert((res.data && res.data.message) || "Failed to unflag review.");
                }
            }).catch(function (err) {
                console.error("unflagReview error:", err);
                alert("Failed to unflag review.");
            });
        };

        $scope.flaggedCount = function () {
            return ($scope.reviews || []).filter(function (r) {
                return r.flagged === true;
            }).length;
        };

        $scope.restoreReview = function (reviewId) {
            $http({
                method: 'POST',
                url: '/Main/RestoreReview',
                data: $httpParamSerializerJQLike({ reviewId: reviewId }),
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
            }).then(function (res) {
                if (res.data.success) {
                    $scope.loadReviews();
                    $scope.loadArchivedReviews();
                } else {
                    alert(res.data.message || "Restore failed.");
                }
            }).catch(function () {
                alert("Restore failed.");
            });
        };

        $scope.archivedReviews = [];

        $scope.loadArchivedReviews = function () {
            $http.get('/Main/GetArchivedReviews')
                .then(function (res) {
                    $scope.archivedReviews = (res.data || []).map(function (r) {
                        return {
                            reviewId: r.ReviewId,
                            rating: r.Rating,
                            reviewText: r.ReviewText,
                            serviceAvailed: r.ServiceAvailed || "Service",
                            date: r.CreatedAt ? new Date(parseInt(r.CreatedAt.substr(6))) : null,
                            images: r.Images || [],
                            flagged: r.Flagged || false,
                            isArchived: true
                        };
                    });
                })
                .catch(function (err) {
                    console.error("Error loading archived reviews:", err);
                });
        };

        $scope.activeTab = 'gallery';

        $scope.setTab = function (tab) {
            $scope.activeTab = tab;
            if (tab === 'reviews') {
                if (!$scope.reviews || $scope.reviews.length === 0) {
                    $scope.loadReviews();
                }
                $scope.updatePagedReviews();
            }
        };

        $scope.parseNotes = function (notes) {
            if (!notes) return {};
            let result = {};
            notes.split('|').forEach(part => {
                let pieces = part.split(':');
                if (pieces.length >= 2) {
                    let key = pieces[0].trim().toLowerCase();
                    let value = pieces.slice(1).join(':').trim();
                    result[key] = value;
                }
            });
            return result;
        };

        // ===== PAGE INITIALIZATION =====
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
        }

        if (currentPath.indexOf('admingallerypage') !== -1) {
            $scope.loadGalleryPhotos();
            $scope.loadReviews();
            $scope.loadArchivedReviews();
        }

        if (currentPath.indexOf('gallerypage') !== -1 && currentPath.indexOf('admin') === -1) {
            $scope.loadPublicGallery();
        }

        if (currentPath.indexOf('gallerypage') !== -1) {
            console.log("Gallery page detected");
            $scope.activeTab = 'gallery';
        }

        // ===== PAYMENT PAGE INITIALIZATION =====
        if (currentPath.indexOf('paymentpage') !== -1) {
            console.log("Payment page detected");
            $scope.loadPaymentInfo();
            $scope.loadPaymentSummary();
        }

        // ===== PAYMENT SETTINGS =====
        $scope.payment = {
            gcash: "",
            bank: ""
        };

        $scope.savePayment = function () {
            var gcash = ($scope.payment.gcash || "").trim();
            var bank = ($scope.payment.bank || "").trim();
            console.log("GCash entered:", gcash, "Length:", gcash.length);
            if (!/^\d{11}$/.test(gcash)) {
                alert("GCash number must be exactly 11 digits.");
                return;
            }
            var fileInput = document.getElementById("qrUpload");
            var file = fileInput && fileInput.files.length > 0 ? fileInput.files[0] : null;
            var formData = new FormData();
            formData.append("gcash", gcash);
            formData.append("bank", bank);
            if (file) {
                formData.append("qr", file);
            }
            $http.post("/Admin/SavePayment", formData, {
                headers: { "Content-Type": undefined },
                transformRequest: angular.identity
            }).then(function (res) {
                console.log("SavePayment response:", res.data);
                alert(res.data.message || "Saved!");
            }).catch(function (err) {
                console.error("SavePayment error:", err);
                alert("Failed to save payment settings.");
            });
        };

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

// ===== FILE MODEL DIRECTIVE FOR PAYMENT PAGE =====
app.directive('fileModel', ['$parse', function ($parse) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            var model = $parse(attrs.fileModel);
            var modelSetter = model.assign;

            element.bind('change', function () {
                scope.$apply(function () {
                    modelSetter(scope, element[0].files[0]);
                });
            });
        }
    };
}]);