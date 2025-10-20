(function () {
    // highlight active nav link
    const path = location.pathname.toLowerCase();
    document.querySelectorAll('.navbar .nav-link')
        .forEach(a => { if (path.startsWith(a.getAttribute('href') || '')) a.classList.add('active'); });

    // smooth scroll to top
    $(document).on('click', '.js-backtotop', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, 300);
    });
})();