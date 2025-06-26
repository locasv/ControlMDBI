// wwwroot/js/registro-personas.js
// Script para manejo de registro de personas (empleados y visitantes)

class RegistroPersonas {
    constructor() {
        this.modoLecturaActivo = true;
        this.init();
    }

    init() {
        // Solo ejecutar si estamos en la página correcta
        if (!this.isRegistroPersonasPage()) {
            return;
        }

        this.setupEventListeners();
        this.initializePage();
    }

    isRegistroPersonasPage() {
        // Verificar si estamos en la página correcta
        return $('#dniEmpleado').length > 0 || $('#registroTabs').length > 0;
    }

    setupEventListeners() {
        // Auto-focus en el campo DNI para lectura de códigos
        $('#dniEmpleado').focus();

        // Controlar habilitación del campo código de objeto para EMPLEADO
        $('#portaObjetoEmpleado').change(() => {
            const portaObjeto = $('#portaObjetoEmpleado').is(':checked');
            $('#codigoObjetoEmpleado').prop('disabled', !portaObjeto);

            if (!portaObjeto) {
                $('#codigoObjetoEmpleado').val('');
            } else {
                $('#codigoObjetoEmpleado').focus();
            }
        });

        // Controlar habilitación del campo código de objeto para VISITANTE
        $('#portaObjetoVisitante').change(() => {
            const portaObjeto = $('#portaObjetoVisitante').is(':checked');
            $('#codigoObjetoVisitante').prop('disabled', !portaObjeto);

            if (!portaObjeto) {
                $('#codigoObjetoVisitante').val('');
            } else {
                $('#codigoObjetoVisitante').focus();
            }
        });

        // Validar solo números en DNI de visitante
        $('#dniVisitante').on('input', function () {
            let valor = this.value.replace(/[^0-9]/g, '');
            if (valor.length > 8) {
                valor = valor.substring(0, 8);
            }
            this.value = valor;
        });

        // Botón limpiar visitante
        $('#btnLimpiarVisitante').click(() => {
            this.limpiarFormularioVisitante();
        });

        // Botón para alternar modo lectura
        $('#btnModoLectura').click(() => {
            this.modoLecturaActivo = !this.modoLecturaActivo;
            this.actualizarModoLectura();
        });

        // Búsqueda manual con botón
        $('#btnBuscarEmpleado').click(() => {
            this.buscarEmpleado();
        });

        // Botón limpiar empleado
        $('#btnLimpiar').click(() => {
            this.limpiarFormularioEmpleado();
        });

        // Búsqueda automática cuando se detecta entrada de 8 dígitos
        $('#dniEmpleado').on('input', (e) => {
            this.handleDniEmpleadoInput(e);
        });

        // Manejar evento de pegado
        $('#dniEmpleado').on('paste', (e) => {
            this.handleDniEmpleadoPaste(e);
        });

        // Validar solo números en tiempo real para empleado
        $('#dniEmpleado').on('input', function () {
            let valor = this.value.replace(/[^0-9]/g, '');
            this.value = valor;
        });

        // Detectar Enter para búsqueda rápida
        $('#dniEmpleado').on('keypress', (e) => {
            if (e.which === 13) {
                e.preventDefault();
                this.buscarEmpleado();
            }
        });

        // Eventos de tabs
        $('#empleado-tab').on('shown.bs.tab', () => {
            if (this.modoLecturaActivo) {
                $('#dniEmpleado').focus();
            }
        });

        $('#visitante-tab').on('shown.bs.tab', () => {
            if (!$('#dniVisitante').val()) {
                $('#dniVisitante').focus();
            }
        });

        // Mantener focus según contexto
        $(document).on('click', (e) => {
            this.handleDocumentClick(e);
        });

        // Permitir navegación normal con Tab
        $('#dniEmpleado').on('keydown', (e) => {
            if (e.which === 9) {
                const empleadoEncontrado = $('#infoEmpleado').is(':visible');
                if (empleadoEncontrado) {
                    return true;
                }
            }
        });
    }

    initializePage() {
        this.actualizarModoLectura();
    }

    limpiarFormularioVisitante() {
        $('#dniVisitante').val('');
        $('#RazonVisita').val('');
        $('#portaObjetoVisitante').prop('checked', false);
        $('#codigoObjetoVisitante').val('').prop('disabled', true);
        $('#dniVisitante').focus();
    }

    actualizarModoLectura() {
        const btn = $('#btnModoLectura');
        const campo = $('#dniEmpleado');

        if (this.modoLecturaActivo) {
            btn.removeClass('btn-outline-warning').addClass('btn-warning');
            btn.attr('title', 'Modo lectura activo - Click para desactivar');
            campo.attr('placeholder', 'Escanee código o ingrese DNI');
            campo.focus();
        } else {
            btn.removeClass('btn-warning').addClass('btn-outline-warning');
            btn.attr('title', 'Modo lectura inactivo - Click para activar');
            campo.attr('placeholder', 'Ingrese DNI manualmente');
        }
    }

    handleDniEmpleadoInput(e) {
        const dni = $(e.target).val();

        this.limpiarResultados();

        if (this.modoLecturaActivo && dni.length === 8) {
            setTimeout(() => {
                this.buscarEmpleado();
            }, 100);
        }

        if (dni.length > 8) {
            const dniLimpio = dni.substring(dni.length - 8);
            $(e.target).val(dniLimpio);
            if (this.modoLecturaActivo) {
                this.buscarEmpleado();
            }
        }
    }

    handleDniEmpleadoPaste(e) {
        setTimeout(() => {
            let valor = $('#dniEmpleado').val();
            valor = valor.replace(/[^0-9]/g, '');
            if (valor.length > 8) {
                valor = valor.substring(valor.length - 8);
            }
            $('#dniEmpleado').val(valor);

            if (valor.length === 8 && this.modoLecturaActivo) {
                this.buscarEmpleado();
            }
        }, 100);
    }

    handleDocumentClick(e) {
        if (!this.modoLecturaActivo || !$('#empleado').hasClass('active')) return;

        const empleadoEncontrado = $('#infoEmpleado').is(':visible');
        const clickEnFormulario = $(e.target).closest('.form-control, .btn, select, .form-select, .form-check, textarea, input, label, .nav-link').length > 0;
        const clickEnModal = $(e.target).closest('.modal, .dropdown-menu').length > 0;

        if (!empleadoEncontrado && !clickEnFormulario && !clickEnModal) {
            const dniActual = $('#dniEmpleado').val();
            if (!dniActual || dniActual.length < 8) {
                $('#dniEmpleado').focus();
            }
        }
    }

    buscarEmpleado() {
        const dni = $('#dniEmpleado').val();

        if (!dni || dni.length !== 8) {
            this.mostrarError('El DNI debe tener exactamente 8 dígitos');
            return;
        }

        $('#loadingEmpleado').show();
        this.limpiarResultados();

        // Necesitamos obtener la URL desde el contexto de la página
        const buscarEmpleadoUrl = window.buscarEmpleadoUrl;//|| '/Vigi/RegistrosPersonas/BuscarEmpleado';

        $.ajax({
            url: buscarEmpleadoUrl,
            type: 'GET',
            data: { dni: dni },
            success: (response) => {
                $('#loadingEmpleado').hide();

                if (response.success) {
                    this.mostrarEmpleado(response.empleado);
                    this.habilitarRegistro(true);
                    this.reproducirSonido('success');
                } else {
                    this.mostrarRedireccionAVisitante(dni, response.message);
                    this.habilitarRegistro(false);
                    this.reproducirSonido('error');
                }
            },
            error: () => {
                $('#loadingEmpleado').hide();
                const dni = $('#dniEmpleado').val();
                this.mostrarRedireccionAVisitante(dni, 'Error al buscar el empleado. Redirigiendo a visitante...');
                this.habilitarRegistro(false);
                this.reproducirSonido('error');
            }
        });
    }

    mostrarRedireccionAVisitante(dni, mensaje) {
        $('#redireccionInfo').show();

        let progreso = 0;
        const intervalo = setInterval(() => {
            progreso += 10;
            $('#redireccionInfo .progress-bar').css('width', progreso + '%');

            if (progreso >= 100) {
                clearInterval(intervalo);

                $('#visitante-tab').tab('show');
                $('#dniVisitante').val(dni);

                setTimeout(() => {
                    $('#RazonVisita').focus();
                }, 500);

                this.mostrarNotificacionRedireccion();

                setTimeout(() => {
                    $('#redireccionInfo').hide();
                    $('#redireccionInfo .progress-bar').css('width', '0%');
                }, 2000);
            }
        }, 200);
    }

    mostrarNotificacionRedireccion() {
        const notificacion = $(`
            <div class="alert alert-success alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                <i class="fas fa-check-circle"></i>
                <strong>Redirección exitosa</strong><br>
                El DNI se ha transferido al registro de visitante.
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);

        $('body').append(notificacion);

        setTimeout(() => {
            notificacion.fadeOut(() => notificacion.remove());
        }, 5000);
    }

    mostrarEmpleado(empleado) {
        $('#nombreEmpleado').text(empleado.nombre + ' ' + empleado.apellido);
        $('#cargoEmpleado').text(empleado.cargo || 'No especificado');
        $('#areaEmpleado').text(empleado.area || 'No especificada');
        $('#estadoEmpleado').text(empleado.estado || 'Activo');
        $('#infoEmpleado').show();

        if (this.modoLecturaActivo) {
            setTimeout(() => {
                $('#dniEmpleado').focus();
            }, 1000);
        } else {
            $('#TipoRegistroEmpleado').focus();
        }
    }

    mostrarError(mensaje) {
        $('#mensajeError').text(mensaje);
        $('#errorEmpleado').show();
    }

    limpiarResultados() {
        $('#infoEmpleado, #errorEmpleado, #loadingEmpleado, #redireccionInfo').hide();
    }

    limpiarFormularioEmpleado() {
        $('#dniEmpleado').val('');
        this.limpiarResultados();
        this.habilitarRegistro(false);
        $('#TipoRegistroEmpleado').val('');
        $('#RazonEmpleado').val('');
        $('#portaObjetoEmpleado').prop('checked', false);
        $('#codigoObjetoEmpleado').val('').prop('disabled', true);
        $('#ObservacionesEmpleado').val('');

        if (this.modoLecturaActivo) {
            $('#dniEmpleado').focus();
        }
    }

    habilitarRegistro(habilitar) {
        $('#btnRegistrarEmpleado').prop('disabled', !habilitar);
    }

    reproducirSonido(tipo) {
        if (typeof AudioContext !== 'undefined' || typeof webkitAudioContext !== 'undefined') {
            try {
                const audioContext = new (AudioContext || webkitAudioContext)();
                const oscillator = audioContext.createOscillator();
                const gainNode = audioContext.createGain();

                oscillator.connect(gainNode);
                gainNode.connect(audioContext.destination);

                if (tipo === 'success') {
                    oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
                    oscillator.frequency.setValueAtTime(1000, audioContext.currentTime + 0.1);
                } else {
                    oscillator.frequency.setValueAtTime(300, audioContext.currentTime);
                }

                gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
                gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);

                oscillator.start(audioContext.currentTime);
                oscillator.stop(audioContext.currentTime + 0.2);
            } catch (e) {
                console.log('Audio no disponible');
            }
        }
    }
}

// Inicializar cuando el documento esté listo
$(document).ready(() => {
    new RegistroPersonas();
});