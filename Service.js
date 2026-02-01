app.service("DestLoungeSalesandBookingService", function ($http) {

    // Method to get booking details
    this.getBookingDetails = function (bookingId) {
        return $http.get('/Main/GetBookingDetails?id=' + bookingId);
    };

    // Method to submit review
    this.submitReview = function (reviewData) {
        return $http.post('/Main/SubmitReview', reviewData);
    };

    // Method to get user profile
    this.getUserProfile = function (userId) {
        return $http.get('/Main/GetUserProfile?id=' + userId);
    };

    // Add other methods as needed
});