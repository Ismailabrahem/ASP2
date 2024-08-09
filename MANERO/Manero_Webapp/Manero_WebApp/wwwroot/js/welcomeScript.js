//welcome script
document.addEventListener("DOMContentLoaded", function () {
    var elementsToFadeIn = document.querySelectorAll(".manero-ring, .welcome-title");
    elementsToFadeIn.forEach(function (element) {
        element.style.opacity = "0";
        fadeIn(element);
    });

    function fadeIn(element) {
        var opacity = 0;
        var timer = setInterval(function () {
            if (opacity >= 1) {
                clearInterval(timer);

                setTimeout(function () {
                    window.location.href = "/welcome";
                }, 3000);
            }
            element.style.opacity = opacity;
            opacity += 0.1;
        }, 100);
    }
});







//getstarted script
document.addEventListener("DOMContentLoaded", function () {
    var elementsToFadeIn = document.querySelectorAll(".big-manero-ring, .getstarted-title, .welcome-text, .get-started-btn");
    elementsToFadeIn.forEach(function (element) {
        element.style.opacity = "0";
        fadeIn(element);
    });

    function fadeIn(element) {
        var opacity = 0;
        var timer = setInterval(function () {
            if (opacity >= 1) {
                clearInterval(timer);
            }
            element.style.opacity = opacity;
            opacity += 0.1;
        }, 100);
    }

    var pages = [
        { title: "Welcome To Manero!", buttonText: "GET STARTED!" },
        { title: "Easy Track Order!", buttonText: "Continue" },
        { title: "Door To Door Delivery!", buttonText: "Sign Up!" }
    ];
    var currentPage = 0;

    var button = document.querySelector(".get-started-btn");
    var title = document.querySelector(".getstarted-title");
    var circles = document.querySelectorAll(".circle");

    function updatePage(pageIndex) {
        title.textContent = pages[pageIndex].title;
        button.textContent = pages[pageIndex].buttonText;
        circles.forEach(function (circle, index) {
            circle.classList.toggle("active", index === pageIndex);
        });
    }

    button.addEventListener("click", function () {
        if (button.textContent === "Sign Up!") {
            window.location.href = "/account/register";
        } else {
            currentPage = (currentPage + 1) % pages.length;
            updatePage(currentPage);
        }
    });

    updatePage(currentPage);
});