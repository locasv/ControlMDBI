$(document).ready(function () {

    //// Verificar si el usuario está autenticado
    const isAuthenticated = estaLogeado;
    // Si no está autenticado, no ejecutar el código del sidebar
    if (!isAuthenticated) {
        return;
    }

    // Variables para control del sidebar
    let sidebarCollapsed = false;

    // Función para alternar el sidebar
    function toggleSidebar() {
        const $sidebar = $('#sidebar');
        const $content = $('#content');
        const $overlay = $('.overlay');

        if (window.innerWidth <= 768) {
            // En móviles, usar overlay
            $sidebar.toggleClass('active');
            $overlay.toggleClass('active');
        } else {
            // En desktop, colapsar/expandir
            sidebarCollapsed = !sidebarCollapsed;
            $sidebar.toggleClass('collapsed', sidebarCollapsed);

            // Guardar estado en localStorage
            localStorage.setItem('sidebarCollapsed', sidebarCollapsed);
        }
    }
    $('#sidebarToggler, #sidebarCollapseInside').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('.overlay').toggleClass('active');
    });

    $('.overlay').on('click', function () {
        $('#sidebar').removeClass('active');
        $('.overlay').removeClass('active');
    });


    // Event listeners para los botones del sidebar
    $('#sidebarToggler, #sidebarCollapseInside').on('click', function (e) {
        e.preventDefault();
        toggleSidebar();
    });

    // Cerrar sidebar cuando se hace clic en overlay (móviles)
    $('.overlay').on('click', function () {
        $('#sidebar').removeClass('active');
        $('.overlay').removeClass('active');
    });

    // Cerrar sidebar al hacer clic en un enlace en móviles
    $('#sidebar .nav-link').on('click', function () {
        if (window.innerWidth <= 768) {
            $('#sidebar').removeClass('active');
            $('.overlay').removeClass('active');
        }
    });

    // Restaurar estado del sidebar desde localStorage
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState === 'true' && window.innerWidth > 768) {
        sidebarCollapsed = true;
        $('#sidebar').addClass('collapsed');
    }

    // Manejar redimensión de ventana
    $(window).on('resize', function () {
        const $sidebar = $('#sidebar');
        const $overlay = $('.overlay');

        if (window.innerWidth > 768) {
            // En desktop, remover clases de móvil
            $sidebar.removeClass('active');
            $overlay.removeClass('active');

            // Aplicar estado guardado
            $sidebar.toggleClass('collapsed', sidebarCollapsed);
        } else {
            // En móvil, remover clase collapsed
            $sidebar.removeClass('collapsed');
        }
    });

    // Marcar como activo el enlace actual
    const currentUrl = window.location.pathname.toLowerCase();
    $('#sidebar .nav-link').each(function () {
        const linkUrl = $(this).attr('href');
        if (linkUrl && currentUrl.includes(linkUrl.toLowerCase()) && linkUrl !== '/') {
            $(this).parent().addClass('active');
        }
    });

    // Efecto de hover mejorado para los enlaces
    $('#sidebar .nav-link').hover(
        function () {
            $(this).find('i').addClass('fa-bounce');
        },
        function () {
            $(this).find('i').removeClass('fa-bounce');
        }
    );

    // Función para scroll suave en el sidebar
    function smoothScrollTo(element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'nearest'
        });
    }

    // Auto-scroll al elemento activo
    const activeLink = $('#sidebar .nav-item.active .nav-link')[0];
    if (activeLink) {
        setTimeout(() => {
            smoothScrollTo(activeLink);
        }, 300);
    }
});

// Función para mostrar datos del perfil (mejorada)
function mostrarDatosPerfil() {

    const isAuthenticated = estaLogeado;

    if (!isAuthenticated) {
        Swal.fire({
            title: 'Error',
            text: 'Debe iniciar sesión para ver el perfil',
            icon: 'warning',
            confirmButtonText: 'Cerrar'
        });
        return;
    }

    console.log("Función mostrarDatosPerfil() ejecutada");

    // Mostrar loading
    Swal.fire({
        title: 'Cargando...',
        text: 'Obteniendo datos del perfil',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    $.ajax({
        url: miPerfilUrl,
        type: 'GET',
        dataType: 'json',
        timeout: 10000, // 10 segundos de timeout
        success: function (response) {
            // Validar respuesta
            if (!response || !response.data) {
                throw new Error('Respuesta inválida del servidor');
            }

            const data = response.data;
            const nombreUsuario = data.nombreUsuario || 'No disponible';
            const rol = data.rol || 'No disponible';
            const nombreEmpleado = data.empleado?.nombres || 'No disponible';
            const apellidosEmpleado = data.empleado?.apellidos || '';
            const nombreCompleto = apellidosEmpleado ?
                `${nombreEmpleado} ${apellidosEmpleado}` : nombreEmpleado;

            Swal.fire({
                title: '<strong>👤 Datos de Perfil</strong>',
                html: `
                    <div style="text-align: left; font-size: 14px; line-height: 1.6;">
                        <div style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
                            <strong style="color: #495057;">👤 Usuario:</strong><br>
                            <span style="color: #6c757d;">${nombreUsuario}</span>
                        </div>
                        <div style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
                            <strong style="color: #495057;">🔐 Rol:</strong><br>
                            <span style="color: #6c757d;">${rol}</span>
                        </div>
                        <div style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
                            <strong style="color: #495057;">👨‍💼 Empleado:</strong><br>
                            <span style="color: #6c757d;">${nombreCompleto}</span>
                        </div>
                    </div>
                `,
                icon: 'info',
                confirmButtonText: '✅ Cerrar',
                confirmButtonColor: '#007bff',
                width: '400px',
                padding: '20px',
                background: '#fff',
                customClass: {
                    popup: 'animated fadeInDown'
                }
            });
        },
        error: function (xhr, status, error) {
            console.error('Error al obtener los datos del perfil:', {
                status: status,
                error: error,
                response: xhr.responseText
            });

            let errorMessage = 'No se pudieron obtener los datos del perfil';

            if (status === 'timeout') {
                errorMessage = 'Tiempo de espera agotado. Intente nuevamente.';
            } else if (xhr.status === 401) {
                errorMessage = 'Sesión expirada. Por favor, inicie sesión nuevamente.';
            } else if (xhr.status === 500) {
                errorMessage = 'Error interno del servidor. Contacte al administrador.';
            }

            Swal.fire({
                title: '❌ Error',
                text: errorMessage,
                icon: 'error',
                confirmButtonText: '🔄 Cerrar',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}