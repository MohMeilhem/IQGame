// Timer sound
let timerSound;
let dotNetHelper;

window.initializeTimerSound = function () {
    timerSound = new Audio('/sounds/timer.mp3');
    timerSound.loop = true;
};

// Visibility handling
window.initializeVisibilityHandler = function (helper) {
    dotNetHelper = helper;
    document.addEventListener('visibilitychange', handleVisibilityChange);
};

function handleVisibilityChange() {
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnVisibilityChange', !document.hidden);
    }
}

// Toast notifications
window.initializeToasts = function () {
    // Initialize toasts if needed
    toastr.options = {
        closeButton: true,
        progressBar: true,
        positionClass: "toast-top-right",
    };
};

window.showToast = function (message) {
    toastr.info(message);
};

window.showConfirmation = function (title, message, confirmText, cancelText) {
    return Swal.fire({
        title: title,
        text: message,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: confirmText || 'Yes',
        cancelButtonText: cancelText || 'No'
    }).then((result) => {
        return result.isConfirmed;
    });
};

// Timer sound control
window.playTimerSound = function () {
    if (timerSound) {
        timerSound.play();
    }
};

window.stopTimerSound = function () {
    if (timerSound) {
        timerSound.pause();
        timerSound.currentTime = 0;
    }
};

// Cleanup when page unloads
window.addEventListener('beforeunload', function() {
    if (dotNetHelper) {
        document.removeEventListener('visibilitychange', handleVisibilityChange);
        dotNetHelper = null;
    }
    if (timerSound) {
        timerSound.pause();
        timerSound = null;
    }
}); 

window.scrollToCategoriesSection = function() {
    var el = document.getElementById('categories-section');
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};