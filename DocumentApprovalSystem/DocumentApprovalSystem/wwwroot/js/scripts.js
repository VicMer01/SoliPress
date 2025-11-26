window.toggleSidebar = () => {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar) {
        sidebar.classList.toggle('open');
    }

    if (overlay) {
        overlay.classList.toggle('show');
    }
};

window.closeSidebar = () => {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');

    if (sidebar) {
        sidebar.classList.remove('open');
    }

    if (overlay) {
        overlay.classList.remove('show');
    }
};
