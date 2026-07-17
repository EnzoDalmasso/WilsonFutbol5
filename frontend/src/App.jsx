import { useEffect, useState } from 'react'
import logoWilson from './assets/LogoWilson.png'

const API_URL = import.meta.env.VITE_API_URL ?? 'https://localhost:7094/api'
const SESSION_TOKEN_DUENO_KEY = 'wilson_token_dueno'

const datosReservaIniciales = {
  apellido: '',
  nombre: '',
  telefonoCliente: '',
}

const datosCambioClaveIniciales = {
  claveActual: '',
  claveNueva: '',
  repetirClaveNueva: '',
}

const opcionesDiasSemana = [
  { valor: '1', texto: 'Lunes' },
  { valor: '2', texto: 'Martes' },
  { valor: '3', texto: 'Miércoles' },
  { valor: '4', texto: 'Jueves' },
  { valor: '5', texto: 'Viernes' },
  { valor: '6', texto: 'Sábado' },
  { valor: '0', texto: 'Domingo' },
]

const opcionesHorasTurnoFijo = Array.from({ length: 24 }, (_, hora) => {
  const horaTexto = String(hora).padStart(2, '0')

  return `${horaTexto}:00`
})

function obtenerFechaHoy() {
  const fecha = new Date()
  const anio = fecha.getFullYear()
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  const dia = String(fecha.getDate()).padStart(2, '0')

  return `${anio}-${mes}-${dia}`
}

function obtenerDatosTurnoFijoIniciales() {
  return {
    apellido: '',
    diaSemana: '1',
    fechaDesde: obtenerFechaHoy(),
    horaInicio: '22:00',
    nombre: '',
    observacion: '',
    telefonoCliente: '',
  }
}

function obtenerDatosReservaEspecialIniciales() {
  return {
    apellido: '',
    fecha: obtenerFechaHoy(),
    horaFin: '15:00',
    horaInicio: '12:00',
    nombre: '',
    observacion: '',
    telefonoCliente: '',
  }
}

function obtenerDatosExcepcionHorarioIniciales() {
  return {
    fechaDesde: obtenerFechaHoy(),
    fechaHasta: obtenerFechaHoy(),
    horaApertura: '18:00',
    horaCierre: '23:00',
    motivo: '',
    tipo: 'cerrado',
  }
}

function formatearHora(fechaIso) {
  return new Intl.DateTimeFormat('es-AR', {
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(fechaIso))
}

function formatearFecha(fechaIso) {
  return new Intl.DateTimeFormat('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(fechaIso))
}

function formatearFechaSimple(fecha) {
  if (!fecha) {
    return '-'
  }

  const fechaNormalizada = fecha.split('T')[0]
  const [anio, mes, dia] = fechaNormalizada.split('-')

  return `${dia}/${mes}/${anio}`
}

function formatearHoraSimple(hora) {
  if (!hora) {
    return '-'
  }

  return hora.slice(0, 5)
}

function normalizarHoraParaApi(hora) {
  return hora.length === 5 ? `${hora}:00` : hora
}

function crearFechaHoraLocal(fecha, hora) {
  return `${fecha}T${hora}`
}

function obtenerTokenAdmin() {
  return sessionStorage.getItem(SESSION_TOKEN_DUENO_KEY) ?? ''
}

function obtenerHeadersAdmin(headers = {}) {
  return {
    ...headers,
    'X-Admin-Token': obtenerTokenAdmin(),
  }
}

function obtenerHoraFinTurnoFijo(horaInicio) {
  const [hora] = horaInicio.split(':')
  const horaFin = (Number(hora) + 1) % 24

  return `${String(horaFin).padStart(2, '0')}:00`
}

function formatearPrecio(valor) {
  return new Intl.NumberFormat('es-AR', {
    currency: 'ARS',
    maximumFractionDigits: 0,
    style: 'currency',
  }).format(valor)
}

function obtenerTelefonoWhatsApp(telefono) {
  const numeros = telefono.replace(/\D/g, '')

  if (!numeros) {
    return ''
  }

  if (numeros.startsWith('54')) {
    return numeros
  }

  // Si el cliente carga un numero local sin codigo de pais, asumimos Argentina.
  return `549${numeros}`
}

function crearLinkWhatsApp(telefono, mensaje) {
  const telefonoWhatsApp = obtenerTelefonoWhatsApp(telefono)

  if (!telefonoWhatsApp) {
    return ''
  }

  return `https://wa.me/${telefonoWhatsApp}?text=${encodeURIComponent(mensaje)}`
}

async function obtenerMensajeError(respuesta) {
  try {
    const datos = await respuesta.json()

    if (datos.mensaje) {
      return datos.mensaje
    }

    if (datos.errors) {
      return Object.values(datos.errors).flat().join(' ')
    }
  } catch {
    // Si la API no devuelve JSON, usamos un mensaje generico para no romper la pantalla.
  }

  return 'No se pudo completar la reserva.'
}

function App() {
  // Guardamos la fecha elegida por el usuario.
  // Cuando cambia, volvemos a consultar la disponibilidad al backend.
  const [fechaSeleccionada, setFechaSeleccionada] = useState(obtenerFechaHoy())

  // Guardamos los horarios que devuelve la API.
  // Arranca vacio porque al cargar la pantalla todavia no pedimos datos.
  const [horarios, setHorarios] = useState([])
  const [disponibilidad, setDisponibilidad] = useState(null)

  // Guardamos el horario que el cliente eligio para reservar.
  // Si vale null, significa que todavia no eligio ningun horario.
  const [horarioSeleccionado, setHorarioSeleccionado] = useState(null)

  // Guardamos los datos del formulario en un solo objeto.
  // Cada input actualiza una propiedad de este estado.
  const [datosReserva, setDatosReserva] = useState(datosReservaIniciales)

  // Estados para controlar carga, errores y mensajes visuales.
  const [cargando, setCargando] = useState(false)
  const [enviandoReserva, setEnviandoReserva] = useState(false)
  const [error, setError] = useState('')
  const [errorReserva, setErrorReserva] = useState('')
  const [turnoConfirmado, setTurnoConfirmado] = useState(null)

  // Control de acceso al panel del dueño.
  // Si existe token en sessionStorage, mantenemos la sesion al recargar la pagina.
  const [modoDueno, setModoDueno] = useState(
    () => Boolean(obtenerTokenAdmin()),
  )
  const [mostrarAccesoDueno, setMostrarAccesoDueno] = useState(
    () => window.location.pathname === '/admin',
  )
  const [claveDueno, setClaveDueno] = useState('')
  const [ingresandoDueno, setIngresandoDueno] = useState(false)
  const [errorAccesoDueno, setErrorAccesoDueno] = useState('')
  const [mensajeAccesoDueno, setMensajeAccesoDueno] = useState('')

  // Guardamos las reservas que el dueno tiene pendientes de revisar.
  const [turnosPendientes, setTurnosPendientes] = useState([])

  // Estados propios del panel del dueno.
  const [cargandoPendientes, setCargandoPendientes] = useState(false)
  const [errorAdmin, setErrorAdmin] = useState('')
  const [mensajeAdmin, setMensajeAdmin] = useState('')
  const [mostrarConfiguracionAdmin, setMostrarConfiguracionAdmin] = useState(false)
  const [panelAdminActivo, setPanelAdminActivo] = useState(null)

  // Guardamos los turnos fijos que ya tiene cargados el dueno.
  const [turnosFijos, setTurnosFijos] = useState([])
  const [cargandoTurnosFijos, setCargandoTurnosFijos] = useState(false)
  const [creandoTurnoFijo, setCreandoTurnoFijo] = useState(false)
  const [desactivandoTurnoFijoId, setDesactivandoTurnoFijoId] = useState(null)
  const [errorTurnosFijos, setErrorTurnosFijos] = useState('')
  const [mensajeTurnosFijos, setMensajeTurnosFijos] = useState('')
  const [datosTurnoFijo, setDatosTurnoFijo] = useState(obtenerDatosTurnoFijoIniciales)

  // Guardamos los datos del formulario de cumpleaños y reservas especiales.
  const [reservasEspeciales, setReservasEspeciales] = useState([])
  const [cargandoReservasEspeciales, setCargandoReservasEspeciales] = useState(false)
  const [creandoReservaEspecial, setCreandoReservaEspecial] = useState(false)
  const [cancelandoReservaEspecialId, setCancelandoReservaEspecialId] = useState(null)
  const [errorReservaEspecial, setErrorReservaEspecial] = useState('')
  const [mensajeReservaEspecial, setMensajeReservaEspecial] = useState('')
  const [datosReservaEspecial, setDatosReservaEspecial] = useState(
    obtenerDatosReservaEspecialIniciales,
  )

  // Guardamos feriados, vacaciones y dias especiales cargados por el dueno.
  const [excepcionesHorario, setExcepcionesHorario] = useState([])
  const [cargandoExcepcionesHorario, setCargandoExcepcionesHorario] = useState(false)
  const [creandoExcepcionHorario, setCreandoExcepcionHorario] = useState(false)
  const [eliminandoExcepcionHorarioId, setEliminandoExcepcionHorarioId] = useState(null)
  const [errorExcepcionesHorario, setErrorExcepcionesHorario] = useState('')
  const [mensajeExcepcionesHorario, setMensajeExcepcionesHorario] = useState('')
  const [datosExcepcionHorario, setDatosExcepcionHorario] = useState(
    obtenerDatosExcepcionHorarioIniciales,
  )

  // Guardamos la configuracion semanal normal de atencion.
  const [horariosAtencion, setHorariosAtencion] = useState([])
  const [cargandoHorariosAtencion, setCargandoHorariosAtencion] = useState(false)
  const [guardandoHorarioAtencionId, setGuardandoHorarioAtencionId] = useState(null)
  const [errorHorariosAtencion, setErrorHorariosAtencion] = useState('')
  const [mensajeHorariosAtencion, setMensajeHorariosAtencion] = useState('')

  // Guardamos precios, sena y datos de transferencia del negocio.
  const [configuracionNegocio, setConfiguracionNegocio] = useState(null)
  const [cargandoConfiguracionNegocio, setCargandoConfiguracionNegocio] = useState(false)
  const [guardandoConfiguracionNegocio, setGuardandoConfiguracionNegocio] = useState(false)
  const [errorConfiguracionNegocio, setErrorConfiguracionNegocio] = useState('')
  const [mensajeConfiguracionNegocio, setMensajeConfiguracionNegocio] = useState('')

  // Datos del formulario para que el dueño pueda cambiar la contraseña del panel.
  const [datosCambioClave, setDatosCambioClave] = useState(datosCambioClaveIniciales)
  const [cambiandoClaveAdmin, setCambiandoClaveAdmin] = useState(false)
  const [errorCambioClaveAdmin, setErrorCambioClaveAdmin] = useState('')
  const [mensajeCambioClaveAdmin, setMensajeCambioClaveAdmin] = useState('')

  async function obtenerDisponibilidad(fecha) {
    setCargando(true)
    setError('')

    try {
      const respuesta = await fetch(`${API_URL}/turnos/disponibilidad?fecha=${fecha}`)

      if (!respuesta.ok) {
        throw new Error('No se pudo obtener la disponibilidad.')
      }

      const datos = await respuesta.json()
      setDisponibilidad(datos)
      setHorarios(datos.horarios ?? [])
    } catch (errorActual) {
      setHorarios([])
      setDisponibilidad(null)
      setError(errorActual.message)
    } finally {
      setCargando(false)
    }
  }

  async function obtenerTurnosPendientesConfirmacion() {
    setCargandoPendientes(true)
    setErrorAdmin('')

    try {
      const respuesta = await fetch(`${API_URL}/turnos/pendientes-confirmacion`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudieron obtener las reservas pendientes.')
      }

      const datos = await respuesta.json()
      setTurnosPendientes(datos)
    } catch (errorActual) {
      setTurnosPendientes([])
      setErrorAdmin(errorActual.message)
    } finally {
      setCargandoPendientes(false)
    }
  }

  async function obtenerReservasEspeciales() {
    setCargandoReservasEspeciales(true)
    setErrorReservaEspecial('')

    try {
      const respuesta = await fetch(`${API_URL}/turnos/reservas-especiales`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudieron obtener las reservas especiales.')
      }

      const datos = await respuesta.json()
      setReservasEspeciales(datos)
    } catch (errorActual) {
      setReservasEspeciales([])
      setErrorReservaEspecial(errorActual.message)
    } finally {
      setCargandoReservasEspeciales(false)
    }
  }

  async function obtenerTurnosFijos() {
    setCargandoTurnosFijos(true)
    setErrorTurnosFijos('')

    try {
      const respuesta = await fetch(`${API_URL}/turnos-fijos`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudieron obtener los turnos fijos.')
      }

      const datos = await respuesta.json()

      // El backend conserva el historial, pero en el panel mostramos solo los turnos fijos activos.
      setTurnosFijos(datos.filter((turnoFijo) => turnoFijo.activo))
    } catch (errorActual) {
      setTurnosFijos([])
      setErrorTurnosFijos(errorActual.message)
    } finally {
      setCargandoTurnosFijos(false)
    }
  }

  async function obtenerExcepcionesHorario() {
    setCargandoExcepcionesHorario(true)
    setErrorExcepcionesHorario('')

    try {
      const respuesta = await fetch(`${API_URL}/excepciones-horario`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudieron obtener las excepciones de horario.')
      }

      const datos = await respuesta.json()
      setExcepcionesHorario(datos)
    } catch (errorActual) {
      setExcepcionesHorario([])
      setErrorExcepcionesHorario(errorActual.message)
    } finally {
      setCargandoExcepcionesHorario(false)
    }
  }

  async function obtenerHorariosAtencion() {
    setCargandoHorariosAtencion(true)
    setErrorHorariosAtencion('')

    try {
      const respuesta = await fetch(`${API_URL}/horarios-atencion`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudieron obtener los horarios de atención.')
      }

      const datos = await respuesta.json()
      setHorariosAtencion(datos)
    } catch (errorActual) {
      setHorariosAtencion([])
      setErrorHorariosAtencion(errorActual.message)
    } finally {
      setCargandoHorariosAtencion(false)
    }
  }

  async function obtenerConfiguracionNegocio() {
    setCargandoConfiguracionNegocio(true)
    setErrorConfiguracionNegocio('')

    try {
      const respuesta = await fetch(`${API_URL}/configuracion-negocio`, {
        headers: obtenerHeadersAdmin(),
      })

      if (!respuesta.ok) {
        throw new Error('No se pudo obtener la configuración del negocio.')
      }

      const datos = await respuesta.json()
      setConfiguracionNegocio(datos)
    } catch (errorActual) {
      setConfiguracionNegocio(null)
      setErrorConfiguracionNegocio(errorActual.message)
    } finally {
      setCargandoConfiguracionNegocio(false)
    }
  }

  async function ejecutarAccionAdmin(turnoId, ruta, mensajeExito) {
    setErrorAdmin('')
    setMensajeAdmin('')

    try {
      const respuesta = await fetch(`${API_URL}${ruta}`, {
        headers: obtenerHeadersAdmin(),
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeAdmin(mensajeExito)

      // Despues de una accion del dueno, refrescamos pendientes y disponibilidad.
      await obtenerTurnosPendientesConfirmacion()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorAdmin(errorActual.message)
    }
  }

  async function confirmarTurnoPendiente(turnoId) {
    // El dueno usa esta accion cuando decide aprobar la reserva.
    await ejecutarAccionAdmin(
      turnoId,
      `/turnos/confirmar/${turnoId}`,
      'Turno confirmado correctamente.',
    )
  }

  async function rechazarTurnoPendiente(turnoId) {
    // El dueno usa esta accion cuando decide liberar el horario.
    await ejecutarAccionAdmin(
      turnoId,
      `/turnos/rechazar-pendiente/${turnoId}`,
      'Reserva pendiente rechazada correctamente.',
    )
  }

  function crearMensajeWhatsAppPendiente(turno, tipoMensaje) {
    const fecha = formatearFecha(turno.fechaHoraInicio)
    const horaInicio = formatearHora(turno.fechaHoraInicio)
    const horaFin = formatearHora(turno.fechaHoraFin)
    const montoSena = formatearPrecio(turno.montoSena)
    const precioTotal = formatearPrecio(turno.precioTotal)
    const alias = configuracionNegocio?.aliasTransferencia
    const titular = configuracionNegocio?.nombreTitularTransferencia
    const datosTransferencia = [
      alias ? `Alias: ${alias}` : '',
      titular ? `Titular: ${titular}` : '',
    ].filter(Boolean)

    if (tipoMensaje === 'sena') {
      return [
        `Hola ${turno.nombreCliente}, te escribimos de Wilson Futbol 5 por tu reserva del ${fecha} de ${horaInicio} a ${horaFin}.`,
        `Para dejarla pendiente de confirmacion, podes enviar una seña de ${montoSena}.`,
        datosTransferencia.length > 0 ? datosTransferencia.join(' - ') : '',
        'Cuando hagas la transferencia, mandanos el comprobante por este chat.',
      ].filter(Boolean).join('\n')
    }

    if (tipoMensaje === 'confirmacion') {
      return [
        `Hola ${turno.nombreCliente}, tu turno en Wilson Futbol 5 queda confirmado para el ${fecha} de ${horaInicio} a ${horaFin}.`,
        `Total de la cancha: ${precioTotal}.`,
        'Te esperamos.',
      ].join('\n')
    }

    return [
      `Hola ${turno.nombreCliente}, te escribimos de Wilson Futbol 5 por tu solicitud del ${fecha} de ${horaInicio} a ${horaFin}.`,
      'No se pudo confirmar el turno, lo sentimos. Wilson Futbol 5',
    ].join('\n')
  }

  function crearLinkWhatsAppPendiente(turno, tipoMensaje) {
    return crearLinkWhatsApp(
      turno.telefonoCliente,
      crearMensajeWhatsAppPendiente(turno, tipoMensaje),
    )
  }

  // useEffect ejecuta una accion cuando cambia una dependencia.
  // En este caso, cada vez que cambia fechaSeleccionada, pedimos horarios nuevos.
  useEffect(() => {
    obtenerDisponibilidad(fechaSeleccionada)
  }, [fechaSeleccionada])

  // Al cargar la pantalla, traemos tambien las reservas pendientes para el panel del dueno.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerTurnosPendientesConfirmacion()
  }, [modoDueno])

  // Al cargar la pantalla, traemos tambien los turnos fijos del dueno.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerTurnosFijos()
  }, [modoDueno])

  // Al cargar la pantalla, traemos los cumpleanos y reservas especiales ya cargadas.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerReservasEspeciales()
  }, [modoDueno])

  // Al cargar la pantalla, traemos feriados, vacaciones y dias especiales.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerExcepcionesHorario()
  }, [modoDueno])

  // Al cargar la pantalla, traemos la semana normal de atencion.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerHorariosAtencion()
  }, [modoDueno])

  // Al cargar la pantalla del dueno, traemos precios y datos de pago.
  useEffect(() => {
    if (!modoDueno) {
      return
    }

    obtenerConfiguracionNegocio()
  }, [modoDueno])

  function cambiarFecha(evento) {
    setFechaSeleccionada(evento.target.value)
    setHorarioSeleccionado(null)
    setErrorReserva('')
    setTurnoConfirmado(null)
  }

  function seleccionarHorario(horario) {
    setHorarioSeleccionado(horario)
    setErrorReserva('')
    setTurnoConfirmado(null)
  }

  function cambiarDatoReserva(evento) {
    const { name, value } = evento.target

    setDatosReserva((datosActuales) => ({
      ...datosActuales,
      [name]: value,
    }))
  }

  function cambiarDatoTurnoFijo(evento) {
    const { name, value } = evento.target

    setDatosTurnoFijo((datosActuales) => ({
      ...datosActuales,
      [name]: value,
    }))
  }

  function cambiarDatoReservaEspecial(evento) {
    const { name, value } = evento.target

    setDatosReservaEspecial((datosActuales) => ({
      ...datosActuales,
      [name]: value,
    }))
  }

  function cambiarDatoExcepcionHorario(evento) {
    const { name, value } = evento.target

    setDatosExcepcionHorario((datosActuales) => ({
      ...datosActuales,
      [name]: value,
    }))
  }

  function cambiarDatoHorarioAtencion(horarioAtencionId, campo, valor) {
    setHorariosAtencion((horariosActuales) =>
      horariosActuales.map((horario) =>
        horario.horarioAtencionId === horarioAtencionId
          ? { ...horario, [campo]: valor }
          : horario,
      ),
    )
  }

  function cambiarDatoConfiguracionNegocio(evento) {
    const { name, value } = evento.target

    setConfiguracionNegocio((configuracionActual) => ({
      ...configuracionActual,
      [name]: value,
    }))
  }

  function cambiarDatoCambioClave(evento) {
    const { name, value } = evento.target

    setDatosCambioClave((datosActuales) => ({
      ...datosActuales,
      [name]: value,
    }))
  }

  async function crearTurnoFijo(evento) {
    evento.preventDefault()
    setErrorTurnosFijos('')
    setMensajeTurnosFijos('')

    setCreandoTurnoFijo(true)

    try {
      const horaFinTurnoFijo = obtenerHoraFinTurnoFijo(datosTurnoFijo.horaInicio)

      const respuesta = await fetch(`${API_URL}/turnos-fijos`, {
        body: JSON.stringify({
          ...datosTurnoFijo,
          diaSemana: Number(datosTurnoFijo.diaSemana),
          fechaHasta: null,
          horaFin: normalizarHoraParaApi(horaFinTurnoFijo),
          horaInicio: normalizarHoraParaApi(datosTurnoFijo.horaInicio),
        }),
        headers: {
          ...obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        },
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeTurnosFijos('Turno fijo creado correctamente.')
      setDatosTurnoFijo(obtenerDatosTurnoFijoIniciales())

      // Refrescamos la lista y la agenda porque un turno fijo nuevo bloquea horarios.
      await obtenerTurnosFijos()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorTurnosFijos(errorActual.message)
    } finally {
      setCreandoTurnoFijo(false)
    }
  }

  async function desactivarTurnoFijo(turnoFijoId) {
    const confirmaDesactivacion = window.confirm('¿Seguro que querés desactivar este turno fijo?')

    if (!confirmaDesactivacion) {
      return
    }

    setErrorTurnosFijos('')
    setMensajeTurnosFijos('')
    setDesactivandoTurnoFijoId(turnoFijoId)

    try {
      const respuesta = await fetch(`${API_URL}/turnos-fijos/${turnoFijoId}/desactivar`, {
        headers: obtenerHeadersAdmin(),
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeTurnosFijos('Turno fijo desactivado correctamente.')

      // Refrescamos la lista y la agenda porque el horario puede volver a quedar disponible.
      await obtenerTurnosFijos()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorTurnosFijos(errorActual.message)
    } finally {
      setDesactivandoTurnoFijoId(null)
    }
  }

  async function crearReservaEspecial(evento) {
    evento.preventDefault()
    setErrorReservaEspecial('')
    setMensajeReservaEspecial('')

    if (datosReservaEspecial.horaFin <= datosReservaEspecial.horaInicio) {
      setErrorReservaEspecial('La hora de fin debe ser posterior a la hora de inicio.')
      return
    }

    setCreandoReservaEspecial(true)

    try {
      const respuesta = await fetch(`${API_URL}/turnos/reserva-especial`, {
        body: JSON.stringify({
          apellido: datosReservaEspecial.apellido,
          fechaHoraFin: crearFechaHoraLocal(datosReservaEspecial.fecha, datosReservaEspecial.horaFin),
          fechaHoraInicio: crearFechaHoraLocal(datosReservaEspecial.fecha, datosReservaEspecial.horaInicio),
          nombre: datosReservaEspecial.nombre,
          observacion: datosReservaEspecial.observacion,
          telefonoCliente: datosReservaEspecial.telefonoCliente,
        }),
        headers: {
          ...obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        },
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeReservaEspecial('Reserva especial creada correctamente.')
      setDatosReservaEspecial(obtenerDatosReservaEspecialIniciales())

      // Refrescamos la agenda para que el bloqueo se vea al instante.
      await obtenerReservasEspeciales()
      await obtenerTurnosPendientesConfirmacion()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorReservaEspecial(errorActual.message)
    } finally {
      setCreandoReservaEspecial(false)
    }
  }

  async function cancelarReservaEspecial(turnoId) {
    const confirmarCancelacion = window.confirm(
      '¿Seguro que querés cancelar esta reserva especial? El horario volverá a quedar disponible.',
    )

    if (!confirmarCancelacion) {
      return
    }

    setErrorReservaEspecial('')
    setMensajeReservaEspecial('')
    setCancelandoReservaEspecialId(turnoId)

    try {
      const respuesta = await fetch(`${API_URL}/turnos/reservas-especiales/${turnoId}/cancelar`, {
        headers: obtenerHeadersAdmin(),
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeReservaEspecial('Reserva especial cancelada correctamente.')

      // Refrescamos la lista y la disponibilidad porque ese bloque vuelve a quedar libre.
      await obtenerReservasEspeciales()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorReservaEspecial(errorActual.message)
    } finally {
      setCancelandoReservaEspecialId(null)
    }
  }

  async function crearExcepcionHorario(evento) {
    evento.preventDefault()
    setErrorExcepcionesHorario('')
    setMensajeExcepcionesHorario('')

    const abierto = datosExcepcionHorario.tipo === 'abierto'

    if (datosExcepcionHorario.fechaHasta < datosExcepcionHorario.fechaDesde) {
      setErrorExcepcionesHorario('La fecha hasta no puede ser menor que la fecha desde.')
      return
    }

    if (abierto && datosExcepcionHorario.horaApertura >= datosExcepcionHorario.horaCierre) {
      setErrorExcepcionesHorario('La hora de apertura debe ser menor a la hora de cierre.')
      return
    }

    setCreandoExcepcionHorario(true)

    try {
      const respuesta = await fetch(`${API_URL}/excepciones-horario`, {
        body: JSON.stringify({
          fechaDesde: datosExcepcionHorario.fechaDesde,
          fechaHasta: datosExcepcionHorario.fechaHasta,
          abierto,
          horaApertura: abierto ? normalizarHoraParaApi(datosExcepcionHorario.horaApertura) : null,
          horaCierre: abierto ? normalizarHoraParaApi(datosExcepcionHorario.horaCierre) : null,
          motivo: datosExcepcionHorario.motivo,
        }),
        headers: {
          ...obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        },
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeExcepcionesHorario('Excepción de horario creada correctamente.')
      setDatosExcepcionHorario(obtenerDatosExcepcionHorarioIniciales())

      // Refrescamos lista y agenda porque la excepcion puede abrir o cerrar una fecha.
      await obtenerExcepcionesHorario()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorExcepcionesHorario(errorActual.message)
    } finally {
      setCreandoExcepcionHorario(false)
    }
  }

  async function eliminarExcepcionHorario(excepcionHorarioId) {
    const confirmaEliminacion = window.confirm('¿Seguro que querés eliminar esta excepción?')

    if (!confirmaEliminacion) {
      return
    }

    setErrorExcepcionesHorario('')
    setMensajeExcepcionesHorario('')
    setEliminandoExcepcionHorarioId(excepcionHorarioId)

    try {
      const respuesta = await fetch(`${API_URL}/excepciones-horario/${excepcionHorarioId}`, {
        headers: obtenerHeadersAdmin(),
        method: 'DELETE',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeExcepcionesHorario('Excepción eliminada correctamente.')

      // Al eliminar, esa fecha vuelve al horario normal semanal.
      await obtenerExcepcionesHorario()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorExcepcionesHorario(errorActual.message)
    } finally {
      setEliminandoExcepcionHorarioId(null)
    }
  }

  async function guardarHorarioAtencion(horario) {
    setErrorHorariosAtencion('')
    setMensajeHorariosAtencion('')

    if (horario.activo && horario.horaApertura >= horario.horaCierre) {
      setErrorHorariosAtencion('La hora de apertura debe ser menor a la hora de cierre.')
      return
    }

    setGuardandoHorarioAtencionId(horario.horarioAtencionId)

    try {
      const respuesta = await fetch(`${API_URL}/horarios-atencion/${horario.horarioAtencionId}`, {
        body: JSON.stringify({
          activo: horario.activo,
          horaApertura: normalizarHoraParaApi(formatearHoraSimple(horario.horaApertura)),
          horaCierre: normalizarHoraParaApi(formatearHoraSimple(horario.horaCierre)),
        }),
        headers: {
          ...obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        },
        method: 'PUT',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      setMensajeHorariosAtencion('Horario actualizado correctamente.')

      // Refrescamos horarios y disponibilidad porque la agenda puede cambiar.
      await obtenerHorariosAtencion()
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorHorariosAtencion(errorActual.message)
    } finally {
      setGuardandoHorarioAtencionId(null)
    }
  }

  async function guardarConfiguracionNegocio(evento) {
    evento.preventDefault()
    setErrorConfiguracionNegocio('')
    setMensajeConfiguracionNegocio('')

    if (!configuracionNegocio) {
      return
    }

    setGuardandoConfiguracionNegocio(true)

    try {
      const respuesta = await fetch(`${API_URL}/configuracion-negocio`, {
        body: JSON.stringify({
          precioPorPersona: Number(configuracionNegocio.precioPorPersona),
          cantidadJugadoresPorTurno: Number(configuracionNegocio.cantidadJugadoresPorTurno),
          montoSena: Number(configuracionNegocio.montoSena),
          aliasTransferencia: configuracionNegocio.aliasTransferencia,
          nombreTitularTransferencia: configuracionNegocio.nombreTitularTransferencia,
          mensajePagoReserva: configuracionNegocio.mensajePagoReserva,
        }),
        headers: {
          ...obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        },
        method: 'PUT',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      const datos = await respuesta.json()

      setConfiguracionNegocio(datos)
      setMensajeConfiguracionNegocio('Configuración actualizada correctamente.')

      // Refrescamos disponibilidad para que el cliente vea el precio actualizado.
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorConfiguracionNegocio(errorActual.message)
    } finally {
      setGuardandoConfiguracionNegocio(false)
    }
  }

  async function reservarTurno(evento) {
    evento.preventDefault()
    setErrorReserva('')
    setTurnoConfirmado(null)

    if (!horarioSeleccionado) {
      setErrorReserva('Selecciona un horario disponible.')
      return
    }

    setEnviandoReserva(true)

    try {
      const respuesta = await fetch(`${API_URL}/turnos/reservar`, {
        body: JSON.stringify({
          ...datosReserva,
          fechaHoraInicio: horarioSeleccionado.fechaHoraInicio,
          tipoTurno: 1,
        }),
        headers: {
          'Content-Type': 'application/json',
        },
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      const turnoReservado = await respuesta.json()

      setTurnoConfirmado(turnoReservado)
      setDatosReserva(datosReservaIniciales)
      setHorarioSeleccionado(null)

      // Volvemos a pedir disponibilidad para que el horario recien reservado quede bloqueado.
      await obtenerDisponibilidad(fechaSeleccionada)
    } catch (errorActual) {
      setErrorReserva(errorActual.message)
    } finally {
      setEnviandoReserva(false)
    }
  }

  function iniciarNuevaReserva() {
    setTurnoConfirmado(null)
    setErrorReserva('')
    setHorarioSeleccionado(null)
  }

  async function ingresarModoDueno(evento) {
    evento.preventDefault()
    setErrorAccesoDueno('')
    setMensajeAccesoDueno('')

    setIngresandoDueno(true)

    try {
      const respuesta = await fetch(`${API_URL}/autenticacion-admin/login`, {
        body: JSON.stringify({
          clave: claveDueno,
        }),
        headers: {
          'Content-Type': 'application/json',
        },
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      const sesion = await respuesta.json()

      // Guardamos solo el token temporal. La contraseña nunca queda guardada en el navegador.
      sessionStorage.setItem(SESSION_TOKEN_DUENO_KEY, sesion.token)
      setModoDueno(true)
      setMostrarAccesoDueno(false)
      setClaveDueno('')
    } catch (errorActual) {
      setErrorAccesoDueno(errorActual.message)
    } finally {
      setIngresandoDueno(false)
    }
  }

  function salirModoDueno() {
    sessionStorage.removeItem(SESSION_TOKEN_DUENO_KEY)
    setModoDueno(false)
    setMostrarAccesoDueno(false)
    setClaveDueno('')
    setErrorAccesoDueno('')
    setMensajeAccesoDueno('')
  }

  async function cambiarClaveAdmin(evento) {
    evento.preventDefault()
    setErrorCambioClaveAdmin('')
    setMensajeCambioClaveAdmin('')

    if (datosCambioClave.claveNueva !== datosCambioClave.repetirClaveNueva) {
      setErrorCambioClaveAdmin('La contraseña nueva y la repeticion no coinciden.')
      return
    }

    setCambiandoClaveAdmin(true)

    try {
      const respuesta = await fetch(`${API_URL}/autenticacion-admin/cambiar-clave`, {
        body: JSON.stringify({
          claveActual: datosCambioClave.claveActual,
          claveNueva: datosCambioClave.claveNueva,
        }),
        headers: obtenerHeadersAdmin({ 'Content-Type': 'application/json' }),
        method: 'POST',
      })

      if (!respuesta.ok) {
        throw new Error(await obtenerMensajeError(respuesta))
      }

      // El backend invalida las sesiones viejas al cambiar contraseña.
      // Por eso cerramos el panel y pedimos que vuelva a ingresar.
      sessionStorage.removeItem(SESSION_TOKEN_DUENO_KEY)
      setDatosCambioClave(datosCambioClaveIniciales)
      setModoDueno(false)
      setMostrarAccesoDueno(true)
      setMensajeAccesoDueno('Contraseña actualizada. Volve a ingresar con la nueva.')
    } catch (errorActual) {
      setErrorCambioClaveAdmin(errorActual.message)
    } finally {
      setCambiandoClaveAdmin(false)
    }
  }

  const precioPorPersona = horarios.length > 0 ? horarios[0].precioPorPersona : 0
  const cantidadDisponibles = horarios.filter((horario) => horario.disponible).length
  const cantidadReservados = horarios.length - cantidadDisponibles
  const esRutaAdmin = window.location.pathname === '/admin'

  return (
    <main className="min-h-screen bg-[#061934] px-4 py-5 text-white">
      <section className="mx-auto flex w-full max-w-6xl flex-col gap-5">
        <header className="rounded-[28px] border border-[#d6a72b]/40 bg-[#0b2f63] p-5 shadow-2xl shadow-black/25">
          <div className="flex flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex items-center gap-4">
              <img
                alt="Wilson Futbol 5"
                className="h-20 w-20 rounded-full border-2 border-[#d6a72b] bg-white object-contain p-1"
                src={logoWilson}
              />
              <div>
                <p className="text-sm font-bold uppercase text-[#d6a72b]">
                  Wilson Futbol 5
                </p>
                <h1 className="mt-1 text-3xl font-black text-white sm:text-4xl">
                  {esRutaAdmin ? 'Panel del dueño' : 'Reserva tu cancha'}
                </h1>
              </div>
            </div>

            {!esRutaAdmin && (
            <div className="grid w-full grid-cols-2 gap-3 rounded-2xl border border-white/10 bg-[#071f43] p-3 sm:w-auto sm:grid-cols-3">
              <div className="rounded-xl bg-white/8 px-3 py-2">
                <p className="text-xs font-semibold text-white/65">Libres</p>
                <p className="mt-1 text-2xl font-black text-[#d6a72b]">{cantidadDisponibles}</p>
              </div>
              <div className="rounded-xl bg-white/8 px-3 py-2">
                <p className="text-xs font-semibold text-white/65">Ocupados</p>
                <p className="mt-1 text-2xl font-black text-white">{cantidadReservados}</p>
              </div>
              <div className="col-span-2 rounded-xl bg-[#d6a72b] px-3 py-2 text-[#061934] sm:col-span-1">
                <p className="text-xs font-bold">Por persona</p>
                <p className="mt-1 text-xl font-black">
                  {precioPorPersona > 0 ? formatearPrecio(precioPorPersona) : '-'}
                </p>
              </div>
            </div>
            )}
          </div>

          {esRutaAdmin && (
          <div className="mt-5 rounded-2xl border border-white/10 bg-[#071f43] p-3">
            {modoDueno ? (
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <p className="text-sm font-bold text-[#d6a72b]">Modo dueño activo</p>
                <div className="flex flex-wrap gap-2">
                  <button
                    className="w-fit rounded-xl border border-[#d6a72b] bg-transparent px-4 py-2 text-sm font-black text-[#d6a72b] transition hover:bg-[#d6a72b] hover:text-[#061934]"
                    onClick={() => setMostrarConfiguracionAdmin((valorActual) => !valorActual)}
                    type="button"
                  >
                    Configuraciones
                  </button>
                  <button
                    className="w-fit rounded-xl border border-white/20 bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff]"
                    onClick={salirModoDueno}
                    type="button"
                  >
                    Salir
                  </button>
                </div>
              </div>
            ) : (
              <div className="grid gap-4 lg:grid-cols-[1fr_auto] lg:items-center">
                <p className="text-sm font-semibold text-white/70">
                  Los administradores pueden acceder al panel del dueño.
                </p>

                {!mostrarAccesoDueno && (
                  <button
                    className="w-fit rounded-xl border border-[#d6a72b] bg-transparent px-4 py-2 text-sm font-black text-[#d6a72b] transition hover:bg-[#d6a72b] hover:text-[#061934]"
                    onClick={() => setMostrarAccesoDueno(true)}
                    type="button"
                  >
                    Acceso dueño
                  </button>
                )}

                {mostrarAccesoDueno && (
                  <form
                    className="grid w-full gap-2 sm:w-auto sm:grid-cols-[220px_auto] sm:items-start"
                    onSubmit={ingresarModoDueno}
                  >
                    <div className="min-w-0">
                      <input
                        className="w-full rounded-xl border border-white/15 bg-white px-3 py-2 text-sm font-semibold text-[#061934] outline-none focus:border-[#d6a72b]"
                        onChange={(evento) => setClaveDueno(evento.target.value)}
                        placeholder="Contraseña dueño"
                        type="password"
                        value={claveDueno}
                      />
                      {mensajeAccesoDueno && (
                        <p className="mt-1 text-xs font-bold text-[#aaf0bd]">
                          {mensajeAccesoDueno}
                        </p>
                      )}
                      {errorAccesoDueno && (
                        <p className="mt-1 text-xs font-bold text-[#ffb4a8]">
                          {errorAccesoDueno}
                        </p>
                      )}
                    </div>

                    <button
                      className="h-10 rounded-xl bg-[#d6a72b] px-5 text-sm font-black text-[#061934] transition hover:bg-[#edc455] disabled:cursor-not-allowed disabled:opacity-60"
                      disabled={ingresandoDueno}
                      type="submit"
                    >
                      {ingresandoDueno ? 'Entrando...' : 'Entrar'}
                    </button>
                  </form>
                )}
              </div>
            )}
          </div>
          )}
        </header>

        {!esRutaAdmin && (
        <section className="grid gap-5 lg:grid-cols-[380px_1fr]">
          <aside className="rounded-[28px] border border-[#d6a72b]/35 bg-[#0b2f63] p-5 shadow-2xl shadow-black/25">
            <div className="rounded-2xl border border-white/10 bg-[#071f43] p-4">
              <label className="text-sm font-bold text-[#d6a72b]" htmlFor="fecha">
                Fecha del turno
              </label>
              <input
                className="mt-2 w-full rounded-xl border border-white/15 bg-white px-3 py-3 text-sm font-semibold text-[#061934] outline-none focus:border-[#d6a72b]"
                id="fecha"
                onChange={cambiarFecha}
                min={obtenerFechaHoy()}
                type="date"
                value={fechaSeleccionada}
              />
            </div>

            <form className="mt-4 rounded-2xl border border-white/10 bg-[#071f43] p-4" onSubmit={reservarTurno}>
              <div className="rounded-xl border border-[#d6a72b]/30 bg-[#102f5d] p-3">
                <p className="text-sm font-bold text-[#d6a72b]">Horario elegido</p>
                <p className="mt-1 text-sm text-white/80">
                  {horarioSeleccionado
                    ? `${formatearHora(horarioSeleccionado.fechaHoraInicio)} a ${formatearHora(horarioSeleccionado.fechaHoraFin)}`
                    : 'Selecciona un horario disponible'}
                </p>
              </div>

              <div className="mt-4 grid gap-3">
                <label className="text-sm font-bold text-white" htmlFor="nombre">
                  Nombre
                </label>
                <input
                  className="rounded-xl border border-white/15 bg-white px-3 py-3 text-sm text-[#061934] outline-none focus:border-[#d6a72b]"
                  id="nombre"
                  maxLength={80}
                  name="nombre"
                  onChange={cambiarDatoReserva}
                  required
                  type="text"
                  value={datosReserva.nombre}
                />

                <label className="text-sm font-bold text-white" htmlFor="apellido">
                  Apellido
                </label>
                <input
                  className="rounded-xl border border-white/15 bg-white px-3 py-3 text-sm text-[#061934] outline-none focus:border-[#d6a72b]"
                  id="apellido"
                  maxLength={80}
                  name="apellido"
                  onChange={cambiarDatoReserva}
                  required
                  type="text"
                  value={datosReserva.apellido}
                />

                <label className="text-sm font-bold text-white" htmlFor="telefonoCliente">
                  Teléfono
                </label>
                <input
                  className="rounded-xl border border-white/15 bg-white px-3 py-3 text-sm text-[#061934] outline-none focus:border-[#d6a72b]"
                  id="telefonoCliente"
                  maxLength={20}
                  name="telefonoCliente"
                  onChange={cambiarDatoReserva}
                  placeholder="+549..."
                  required
                  type="tel"
                  value={datosReserva.telefonoCliente}
                />
              </div>

              {errorReserva && (
                <p className="mt-4 rounded-xl border border-[#ffb4a8] bg-[#fff3f0] p-3 text-sm font-semibold text-[#9a2f22]">
                  {errorReserva}
                </p>
              )}

              {turnoConfirmado && (
                <div className="mt-4 rounded-xl border border-[#d6a72b] bg-white p-4 text-sm text-[#061934]">
                  <p className="text-lg font-black text-[#0b2f63]">
                    Reserva enviada al dueño
                  </p>
                  <dl className="mt-3 grid gap-2">
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Fecha</dt>
                      <dd>{formatearFecha(turnoConfirmado.fechaHoraInicio)}</dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Horario</dt>
                      <dd>
                        {formatearHora(turnoConfirmado.fechaHoraInicio)} a{' '}
                        {formatearHora(turnoConfirmado.fechaHoraFin)}
                      </dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Cliente</dt>
                      <dd>{turnoConfirmado.nombreCliente}</dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Teléfono</dt>
                      <dd>{turnoConfirmado.telefonoCliente}</dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Monto sugerido</dt>
                      <dd>{formatearPrecio(turnoConfirmado.montoSena)}</dd>
                    </div>
                  </dl>

                  <div className="mt-4 rounded-xl border border-[#d6a72b]/60 bg-[#fff8e7] p-3">
                    <p className="text-sm font-black uppercase text-[#0b2f63]">
                      Datos para transferir
                    </p>
                    <dl className="mt-3 grid gap-2">
                      {turnoConfirmado.aliasTransferencia && (
                        <div>
                          <dt className="font-bold text-[#0b2f63]">Alias</dt>
                          <dd className="text-base font-black">
                            {turnoConfirmado.aliasTransferencia}
                          </dd>
                        </div>
                      )}
                      {turnoConfirmado.nombreTitularTransferencia && (
                        <div>
                          <dt className="font-bold text-[#0b2f63]">Titular</dt>
                          <dd>{turnoConfirmado.nombreTitularTransferencia}</dd>
                        </div>
                      )}
                      {turnoConfirmado.mensajePagoReserva && (
                        <div>
                          <dt className="font-bold text-[#0b2f63]">Mensaje</dt>
                          <dd>{turnoConfirmado.mensajePagoReserva}</dd>
                        </div>
                      )}
                    </dl>
                  </div>

                  <button
                    className="mt-4 w-full rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff]"
                    onClick={iniciarNuevaReserva}
                    type="button"
                  >
                    Nueva reserva
                  </button>
                </div>
              )}

              <button
                className="mt-4 w-full rounded-xl bg-[#d6a72b] px-4 py-3 text-sm font-black text-[#061934] transition hover:bg-[#edc455] disabled:cursor-not-allowed disabled:bg-white/25 disabled:text-white/55"
                disabled={!horarioSeleccionado || enviandoReserva}
                type="submit"
              >
                {enviandoReserva ? 'Confirmando...' : 'Confirmar reserva'}
              </button>
            </form>
          </aside>

          <section className="rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
            <div className="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <p className="text-sm font-bold uppercase text-[#d6a72b]">
                  Agenda
                </p>
                <h2 className="text-2xl font-black text-[#0b2f63]">Horarios disponibles</h2>
              </div>
              {cargando && <p className="text-sm font-bold text-[#0b2f63]">Cargando...</p>}
            </div>

            {error && (
              <div className="mt-5 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
                {error}
              </div>
            )}

            {!cargando && !error && disponibilidad && !disponibilidad.abierto && (
              <div className="mt-5 rounded-xl border border-[#d6a72b]/50 bg-[#fff8e7] p-4 text-sm font-semibold text-[#7a5200]">
                {disponibilidad.motivoNoDisponible ?? 'La cancha no abre en esta fecha.'}
              </div>
            )}

            {!cargando && !error && disponibilidad?.abierto && horarios.length === 0 && (
              <div className="mt-5 rounded-xl border border-[#d6dce5] bg-[#f6f8fb] p-4 text-sm font-semibold text-[#566273]">
                No hay horarios cargados para esta fecha.
              </div>
            )}

            <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {horarios.map((horario) => {
                const estaSeleccionado =
                  horarioSeleccionado?.fechaHoraInicio === horario.fechaHoraInicio

                return (
                  <button
                    className={`min-h-28 rounded-2xl border px-4 py-4 text-left transition ${
                      horario.disponible
                        ? 'border-[#d6a72b]/70 bg-[#fffaf0] hover:border-[#0b2f63]'
                        : 'cursor-not-allowed border-[#d7dde7] bg-[#eef1f5] text-[#7c8797]'
                    } ${estaSeleccionado ? 'border-[#0b2f63] ring-2 ring-[#d6a72b]/40' : ''}`}
                    disabled={!horario.disponible}
                    key={horario.fechaHoraInicio}
                    onClick={() => seleccionarHorario(horario)}
                    type="button"
                  >
                    <span className="block text-2xl font-black">
                      {formatearHora(horario.fechaHoraInicio)}
                    </span>
                    <span
                      className={`mt-3 inline-flex rounded-full px-3 py-1 text-xs font-black ${
                        horario.disponible
                          ? 'bg-[#0b2f63] text-white'
                          : 'bg-white text-[#7c8797]'
                      }`}
                    >
                      {horario.disponible ? 'Disponible' : horario.textoEstado ?? 'Reservado'}
                    </span>
                  </button>
                )
              })}
            </div>
          </section>
        </section>
        )}

        {esRutaAdmin && modoDueno && (
          <>
        {mostrarConfiguracionAdmin && (
        <section className="order-[1] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div>
            <p className="text-sm font-bold uppercase text-[#d6a72b]">
              Panel del dueño
            </p>
            <h2 className="text-2xl font-black text-[#0b2f63]">
              Seguridad del panel
            </h2>
          </div>

          <form
            className="mt-5 rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
            onSubmit={cambiarClaveAdmin}
          >
            <div className="grid gap-3 md:grid-cols-3">
              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="claveActual">
                  Contraseña actual
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="claveActual"
                  name="claveActual"
                  onChange={cambiarDatoCambioClave}
                  required
                  type="password"
                  value={datosCambioClave.claveActual}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="claveNueva">
                  Nueva contraseña
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="claveNueva"
                  minLength={8}
                  name="claveNueva"
                  onChange={cambiarDatoCambioClave}
                  required
                  type="password"
                  value={datosCambioClave.claveNueva}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="repetirClaveNueva">
                  Repetir contraseña
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="repetirClaveNueva"
                  minLength={8}
                  name="repetirClaveNueva"
                  onChange={cambiarDatoCambioClave}
                  required
                  type="password"
                  value={datosCambioClave.repetirClaveNueva}
                />
              </div>
            </div>

            {mensajeCambioClaveAdmin && (
              <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
                {mensajeCambioClaveAdmin}
              </p>
            )}

            {errorCambioClaveAdmin && (
              <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
                {errorCambioClaveAdmin}
              </p>
            )}

            <button
              className="mt-4 rounded-xl bg-[#0b2f63] px-4 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cambiandoClaveAdmin}
              type="submit"
            >
              {cambiandoClaveAdmin ? 'Guardando...' : 'Cambiar contraseña'}
            </button>
          </form>
        </section>
        )}

        {panelAdminActivo === 'reservasEspeciales' && (
        <section className="order-[4] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div>
            <p className="text-sm font-bold uppercase text-[#d6a72b]">
              Panel del dueño
            </p>
            <h2 className="text-2xl font-black text-[#0b2f63]">
              Cumpleaños y reservas especiales
            </h2>
          </div>

          <button
            className="mt-4 rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
            disabled={cargandoReservasEspeciales}
            onClick={obtenerReservasEspeciales}
            type="button"
          >
            {cargandoReservasEspeciales ? 'Actualizando...' : 'Actualizar'}
          </button>

          <form
            className="mt-5 rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
            onSubmit={crearReservaEspecial}
          >
            <p className="text-sm font-black uppercase text-[#d6a72b]">
              Nueva reserva especial
            </p>

            <div className="mt-4 grid gap-3 md:grid-cols-2 lg:grid-cols-4">
              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialNombre">
                  Nombre
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialNombre"
                  maxLength={80}
                  name="nombre"
                  onChange={cambiarDatoReservaEspecial}
                  required
                  type="text"
                  value={datosReservaEspecial.nombre}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialApellido">
                  Apellido
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialApellido"
                  maxLength={80}
                  name="apellido"
                  onChange={cambiarDatoReservaEspecial}
                  required
                  type="text"
                  value={datosReservaEspecial.apellido}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialTelefono">
                  Teléfono
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialTelefono"
                  maxLength={20}
                  name="telefonoCliente"
                  onChange={cambiarDatoReservaEspecial}
                  placeholder="+549..."
                  required
                  type="tel"
                  value={datosReservaEspecial.telefonoCliente}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialFecha">
                  Fecha
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialFecha"
                  min={obtenerFechaHoy()}
                  name="fecha"
                  onChange={cambiarDatoReservaEspecial}
                  required
                  type="date"
                  value={datosReservaEspecial.fecha}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialHoraInicio">
                  Inicio
                </label>
                <select
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialHoraInicio"
                  name="horaInicio"
                  onChange={cambiarDatoReservaEspecial}
                  value={datosReservaEspecial.horaInicio}
                >
                  {opcionesHorasTurnoFijo.map((hora) => (
                    <option key={hora} value={hora}>
                      {hora}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialHoraFin">
                  Fin
                </label>
                <select
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="reservaEspecialHoraFin"
                  name="horaFin"
                  onChange={cambiarDatoReservaEspecial}
                  value={datosReservaEspecial.horaFin}
                >
                  {opcionesHorasTurnoFijo.map((hora) => (
                    <option key={hora} value={hora}>
                      {hora}
                    </option>
                  ))}
                </select>
              </div>
            </div>

            <div className="mt-3">
              <label className="text-sm font-bold text-[#0b2f63]" htmlFor="reservaEspecialObservacion">
                Observación
              </label>
              <textarea
                className="mt-1 min-h-20 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                id="reservaEspecialObservacion"
                maxLength={250}
                name="observacion"
                onChange={cambiarDatoReservaEspecial}
                placeholder="Cumpleaños, evento familiar, pago en efectivo..."
                value={datosReservaEspecial.observacion}
              />
            </div>

            {mensajeReservaEspecial && (
              <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
                {mensajeReservaEspecial}
              </p>
            )}

            {errorReservaEspecial && (
              <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
                {errorReservaEspecial}
              </p>
            )}

            <button
              className="mt-4 rounded-xl bg-[#0b2f63] px-4 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={creandoReservaEspecial}
              type="submit"
            >
              {creandoReservaEspecial ? 'Creando...' : 'Crear reserva especial'}
            </button>
          </form>

          {!cargandoReservasEspeciales && reservasEspeciales.length === 0 && (
            <div className="mt-5 rounded-xl border border-[#d6dce5] bg-[#f6f8fb] p-4 text-sm font-semibold text-[#566273]">
              No hay reservas especiales cargadas.
            </div>
          )}

          <div className="mt-5 grid gap-4 lg:grid-cols-2">
            {reservasEspeciales.map((reservaEspecial) => (
              <article
                className="rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
                key={reservaEspecial.turnoId}
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="text-xs font-black uppercase text-[#d6a72b]">
                      Cumpleaños
                    </p>
                    <h3 className="mt-1 text-xl font-black text-[#0b2f63]">
                      {reservaEspecial.nombreCliente}
                    </h3>
                    <p className="mt-1 text-sm font-semibold text-[#566273]">
                      {reservaEspecial.telefonoCliente}
                    </p>
                  </div>

                  <span className="inline-flex w-fit rounded-full bg-[#e9f2ff] px-3 py-1 text-xs font-black text-[#0b2f63]">
                    Reservado
                  </span>
                </div>

                <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="font-bold text-[#0b2f63]">Horario</dt>
                    <dd>
                      {formatearHora(reservaEspecial.fechaHoraInicio)} a{' '}
                      {formatearHora(reservaEspecial.fechaHoraFin)}
                    </dd>
                  </div>
                  <div>
                    <dt className="font-bold text-[#0b2f63]">Fecha</dt>
                    <dd>{formatearFecha(reservaEspecial.fechaHoraInicio)}</dd>
                  </div>
                  {reservaEspecial.observacion && (
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Observación</dt>
                      <dd>{reservaEspecial.observacion}</dd>
                    </div>
                  )}
                </dl>

                <button
                  className="mt-4 rounded-xl border border-[#9a3d2d] bg-white px-3 py-2 text-sm font-black text-[#9a3d2d] transition hover:bg-[#fff5f2] disabled:cursor-not-allowed disabled:opacity-50"
                  disabled={cancelandoReservaEspecialId === reservaEspecial.turnoId}
                  onClick={() => cancelarReservaEspecial(reservaEspecial.turnoId)}
                  type="button"
                >
                  {cancelandoReservaEspecialId === reservaEspecial.turnoId
                    ? 'Cancelando...'
                    : 'Cancelar reserva especial'}
                </button>
              </article>
            ))}
          </div>
        </section>
        )}

        {panelAdminActivo === 'configuracion' && (
        <section className="order-[4] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-bold uppercase text-[#d6a72b]">
                Panel del dueño
              </p>
              <h2 className="text-2xl font-black text-[#0b2f63]">
                Configuración del negocio
              </h2>
            </div>

            <button
              className="rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cargandoConfiguracionNegocio}
              onClick={obtenerConfiguracionNegocio}
              type="button"
            >
              {cargandoConfiguracionNegocio ? 'Actualizando...' : 'Actualizar'}
            </button>
          </div>

          {mensajeConfiguracionNegocio && (
            <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
              {mensajeConfiguracionNegocio}
            </p>
          )}

          {errorConfiguracionNegocio && (
            <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
              {errorConfiguracionNegocio}
            </p>
          )}

          {configuracionNegocio && (
            <form
              className="mt-5 rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
              onSubmit={guardarConfiguracionNegocio}
            >
              <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-4">
                <div>
                  <label className="text-sm font-bold text-[#0b2f63]" htmlFor="precioPorPersona">
                    Precio por persona
                  </label>
                  <input
                    className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                    id="precioPorPersona"
                    min="0"
                    name="precioPorPersona"
                    onChange={cambiarDatoConfiguracionNegocio}
                    required
                    type="number"
                    value={configuracionNegocio.precioPorPersona}
                  />
                </div>

                <div>
                  <label className="text-sm font-bold text-[#0b2f63]" htmlFor="cantidadJugadoresPorTurno">
                    Jugadores por turno
                  </label>
                  <input
                    className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                    id="cantidadJugadoresPorTurno"
                    min="1"
                    name="cantidadJugadoresPorTurno"
                    onChange={cambiarDatoConfiguracionNegocio}
                    required
                    type="number"
                    value={configuracionNegocio.cantidadJugadoresPorTurno}
                  />
                </div>

                <div>
                  <label className="text-sm font-bold text-[#0b2f63]" htmlFor="montoSena">
                    Seña / reserva
                  </label>
                  <input
                    className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                    id="montoSena"
                    min="0"
                    name="montoSena"
                    onChange={cambiarDatoConfiguracionNegocio}
                    required
                    type="number"
                    value={configuracionNegocio.montoSena}
                  />
                </div>

                <div className="rounded-xl bg-[#d6a72b] px-3 py-2 text-[#061934]">
                  <p className="text-xs font-bold">Total cancha</p>
                  <p className="mt-1 text-xl font-black">
                    {formatearPrecio(
                      Number(configuracionNegocio.precioPorPersona) *
                        Number(configuracionNegocio.cantidadJugadoresPorTurno),
                    )}
                  </p>
                </div>
              </div>

              <div className="mt-4 grid gap-3 md:grid-cols-2">
                <div>
                  <label className="text-sm font-bold text-[#0b2f63]" htmlFor="aliasTransferencia">
                    Alias
                  </label>
                  <input
                    className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                    id="aliasTransferencia"
                    maxLength={100}
                    name="aliasTransferencia"
                    onChange={cambiarDatoConfiguracionNegocio}
                    type="text"
                    value={configuracionNegocio.aliasTransferencia}
                  />
                </div>

                <div>
                  <label className="text-sm font-bold text-[#0b2f63]" htmlFor="nombreTitularTransferencia">
                    Titular
                  </label>
                  <input
                    className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                    id="nombreTitularTransferencia"
                    maxLength={120}
                    name="nombreTitularTransferencia"
                    onChange={cambiarDatoConfiguracionNegocio}
                    type="text"
                    value={configuracionNegocio.nombreTitularTransferencia}
                  />
                </div>
              </div>

              <div className="mt-4">
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="mensajePagoReserva">
                  Mensaje de pago
                </label>
                <textarea
                  className="mt-1 min-h-20 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="mensajePagoReserva"
                  maxLength={300}
                  name="mensajePagoReserva"
                  onChange={cambiarDatoConfiguracionNegocio}
                  value={configuracionNegocio.mensajePagoReserva}
                />
              </div>

              <button
                className="mt-4 rounded-xl bg-[#0b2f63] px-4 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
                disabled={guardandoConfiguracionNegocio}
                type="submit"
              >
                {guardandoConfiguracionNegocio ? 'Guardando...' : 'Guardar configuración'}
              </button>
            </form>
          )}
        </section>
        )}

        <section className="order-[2] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-bold uppercase text-[#d6a72b]">
                Panel del dueño
              </p>
              <h2 className="text-2xl font-black text-[#0b2f63]">
                Reservas pendientes de aprobación
              </h2>
            </div>

            <button
              className="rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cargandoPendientes}
              onClick={obtenerTurnosPendientesConfirmacion}
              type="button"
            >
              {cargandoPendientes ? 'Actualizando...' : 'Actualizar'}
            </button>
          </div>

          {mensajeAdmin && (
            <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
              {mensajeAdmin}
            </p>
          )}

          {errorAdmin && (
            <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
              {errorAdmin}
            </p>
          )}

          {!cargandoPendientes && turnosPendientes.length === 0 && (
            <div className="mt-5 rounded-xl border border-[#d6dce5] bg-[#f6f8fb] p-4 text-sm font-semibold text-[#566273]">
              No hay reservas pendientes para revisar.
            </div>
          )}

          <div className="mt-5 grid gap-4 lg:grid-cols-2">
            {turnosPendientes.map((turno) => (
                <article
                  className="rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
                  key={turno.turnoId}
                >
                  <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                    <div>
                      <p className="text-xs font-black uppercase text-[#d6a72b]">
                        Turno #{turno.turnoId}
                      </p>
                      <h3 className="mt-1 text-xl font-black text-[#0b2f63]">
                        {turno.nombreCliente}
                      </h3>
                      <p className="mt-1 text-sm font-semibold text-[#566273]">
                        {turno.telefonoCliente}
                      </p>
                    </div>

                    <span
                      className="inline-flex w-fit rounded-full bg-[#e9f2ff] px-3 py-1 text-xs font-black text-[#0b2f63]"
                    >
                      {turno.textoEstado}
                    </span>
                  </div>

                  <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Fecha</dt>
                      <dd>{formatearFecha(turno.fechaHoraInicio)}</dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Horario</dt>
                      <dd>
                        {formatearHora(turno.fechaHoraInicio)} a{' '}
                        {formatearHora(turno.fechaHoraFin)}
                      </dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Monto sugerido</dt>
                      <dd>{formatearPrecio(turno.montoSena)}</dd>
                    </div>
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Total</dt>
                      <dd>{formatearPrecio(turno.precioTotal)}</dd>
                    </div>
                  </dl>

                  <div className="mt-4 rounded-xl border border-[#d6dce5] bg-white p-3">
                    <p className="text-xs font-black uppercase text-[#d6a72b]">
                      WhatsApp manual
                    </p>
                    <div className="mt-3 grid gap-2 sm:grid-cols-3">
                      <a
                        className="rounded-xl border border-[#1e6b35] bg-[#f0fff4] px-3 py-2 text-center text-sm font-black text-[#1e6b35] transition hover:bg-[#d7f8df]"
                        href={crearLinkWhatsAppPendiente(turno, 'sena')}
                        rel="noreferrer"
                        target="_blank"
                      >
                        Pedir seña
                      </a>
                      <a
                        className="rounded-xl border border-[#0b2f63] bg-[#edf3ff] px-3 py-2 text-center text-sm font-black text-[#0b2f63] transition hover:bg-[#dbe8ff]"
                        href={crearLinkWhatsAppPendiente(turno, 'confirmacion')}
                        rel="noreferrer"
                        target="_blank"
                      >
                        Avisar confirmado
                      </a>
                      <a
                        className="rounded-xl border border-[#9a3d2d] bg-[#fff5f2] px-3 py-2 text-center text-sm font-black text-[#9a3d2d] transition hover:bg-[#ffe6df]"
                        href={crearLinkWhatsAppPendiente(turno, 'rechazo')}
                        rel="noreferrer"
                        target="_blank"
                      >
                        Avisar rechazo
                      </a>
                    </div>
                  </div>

                  <div className="mt-4 grid gap-2 sm:grid-cols-2">
                    <button
                      className="rounded-xl bg-[#0b2f63] px-3 py-2 text-sm font-black text-white transition hover:bg-[#164d95]"
                      onClick={() => confirmarTurnoPendiente(turno.turnoId)}
                      type="button"
                    >
                      Confirmar
                    </button>
                    <button
                      className="rounded-xl border border-[#9a3d2d] bg-white px-3 py-2 text-sm font-black text-[#9a3d2d] transition hover:bg-[#fff5f2]"
                      onClick={() => rechazarTurnoPendiente(turno.turnoId)}
                      type="button"
                    >
                      Rechazar
                    </button>
                  </div>
                </article>
            ))}
          </div>
        </section>

        <section className="order-[3] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <p className="text-sm font-bold uppercase text-[#d6a72b]">
            Administración
          </p>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            {[
              { clave: 'horarios', texto: 'Horarios de atención' },
              { clave: 'configuracion', texto: 'Configuración del negocio' },
              { clave: 'turnosFijos', texto: 'Turnos fijos' },
              { clave: 'excepciones', texto: 'Feriados y excepciones' },
              { clave: 'reservasEspeciales', texto: 'Cumpleaños y reservas especiales' },
            ].map((panel) => (
              <button
                className={`rounded-xl border px-4 py-3 text-sm font-black transition ${
                  panelAdminActivo === panel.clave
                    ? 'border-[#0b2f63] bg-[#0b2f63] text-white'
                    : 'border-[#d6dce5] bg-[#f8fafc] text-[#0b2f63] hover:border-[#d6a72b]'
                }`}
                key={panel.clave}
                onClick={() =>
                  setPanelAdminActivo((panelActual) =>
                    panelActual === panel.clave ? null : panel.clave,
                  )
                }
                type="button"
              >
                {panel.texto}
              </button>
            ))}
          </div>
        </section>

        {panelAdminActivo === 'horarios' && (
        <section className="order-[4] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-bold uppercase text-[#d6a72b]">
                Panel del dueño
              </p>
              <h2 className="text-2xl font-black text-[#0b2f63]">
                Horarios de atención
              </h2>
            </div>

            <button
              className="rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cargandoHorariosAtencion}
              onClick={obtenerHorariosAtencion}
              type="button"
            >
              {cargandoHorariosAtencion ? 'Actualizando...' : 'Actualizar'}
            </button>
          </div>

          {mensajeHorariosAtencion && (
            <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
              {mensajeHorariosAtencion}
            </p>
          )}

          {errorHorariosAtencion && (
            <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
              {errorHorariosAtencion}
            </p>
          )}

          <div className="mt-5 grid gap-4 lg:grid-cols-2">
            {horariosAtencion.map((horario) => (
              <article
                className="rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
                key={horario.horarioAtencionId}
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="text-xs font-black uppercase text-[#d6a72b]">
                      Día semanal
                    </p>
                    <h3 className="mt-1 text-xl font-black text-[#0b2f63]">
                      {horario.diaSemanaTexto}
                    </h3>
                  </div>

                  <label className="inline-flex w-fit items-center gap-2 rounded-full bg-[#e9f2ff] px-3 py-1 text-xs font-black text-[#0b2f63]">
                    <input
                      checked={horario.activo}
                      className="h-4 w-4 accent-[#0b2f63]"
                      onChange={(evento) =>
                        cambiarDatoHorarioAtencion(
                          horario.horarioAtencionId,
                          'activo',
                          evento.target.checked,
                        )
                      }
                      type="checkbox"
                    />
                    {horario.activo ? 'Abierto' : 'Cerrado'}
                  </label>
                </div>

                <div className="mt-4 grid gap-3 sm:grid-cols-2">
                  <div>
                    <label
                      className="text-sm font-bold text-[#0b2f63]"
                      htmlFor={`apertura-${horario.horarioAtencionId}`}
                    >
                      Apertura
                    </label>
                    <select
                      className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b] disabled:bg-[#eef1f5]"
                      disabled={!horario.activo}
                      id={`apertura-${horario.horarioAtencionId}`}
                      onChange={(evento) =>
                        cambiarDatoHorarioAtencion(
                          horario.horarioAtencionId,
                          'horaApertura',
                          evento.target.value,
                        )
                      }
                      value={formatearHoraSimple(horario.horaApertura)}
                    >
                      {opcionesHorasTurnoFijo.map((hora) => (
                        <option key={hora} value={hora}>
                          {hora}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label
                      className="text-sm font-bold text-[#0b2f63]"
                      htmlFor={`cierre-${horario.horarioAtencionId}`}
                    >
                      Cierre
                    </label>
                    <select
                      className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b] disabled:bg-[#eef1f5]"
                      disabled={!horario.activo}
                      id={`cierre-${horario.horarioAtencionId}`}
                      onChange={(evento) =>
                        cambiarDatoHorarioAtencion(
                          horario.horarioAtencionId,
                          'horaCierre',
                          evento.target.value,
                        )
                      }
                      value={formatearHoraSimple(horario.horaCierre)}
                    >
                      {opcionesHorasTurnoFijo.map((hora) => (
                        <option key={hora} value={hora}>
                          {hora}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>

                <button
                  className="mt-4 rounded-xl bg-[#0b2f63] px-3 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
                  disabled={guardandoHorarioAtencionId === horario.horarioAtencionId}
                  onClick={() => guardarHorarioAtencion(horario)}
                  type="button"
                >
                  {guardandoHorarioAtencionId === horario.horarioAtencionId
                    ? 'Guardando...'
                    : 'Guardar horario'}
                </button>
              </article>
            ))}
          </div>
        </section>
        )}

        {panelAdminActivo === 'turnosFijos' && (
        <section className="order-[4] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-bold uppercase text-[#d6a72b]">
                Panel del dueño
              </p>
              <h2 className="text-2xl font-black text-[#0b2f63]">Turnos fijos</h2>
            </div>

            <button
              className="rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cargandoTurnosFijos}
              onClick={obtenerTurnosFijos}
              type="button"
            >
              {cargandoTurnosFijos ? 'Actualizando...' : 'Actualizar'}
            </button>
          </div>

          <form
            className="mt-5 rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
            onSubmit={crearTurnoFijo}
          >
            <p className="text-sm font-black uppercase text-[#d6a72b]">Nuevo turno fijo</p>

            <div className="mt-4 grid gap-3 md:grid-cols-2 lg:grid-cols-4">
              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoNombre">
                  Nombre
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoNombre"
                  maxLength={80}
                  name="nombre"
                  onChange={cambiarDatoTurnoFijo}
                  required
                  type="text"
                  value={datosTurnoFijo.nombre}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoApellido">
                  Apellido
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoApellido"
                  maxLength={80}
                  name="apellido"
                  onChange={cambiarDatoTurnoFijo}
                  required
                  type="text"
                  value={datosTurnoFijo.apellido}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoTelefono">
                  Teléfono
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoTelefono"
                  maxLength={20}
                  name="telefonoCliente"
                  onChange={cambiarDatoTurnoFijo}
                  placeholder="+549..."
                  required
                  type="tel"
                  value={datosTurnoFijo.telefonoCliente}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoDia">
                  Día
                </label>
                <select
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoDia"
                  name="diaSemana"
                  onChange={cambiarDatoTurnoFijo}
                  value={datosTurnoFijo.diaSemana}
                >
                  {opcionesDiasSemana.map((dia) => (
                    <option key={dia.valor} value={dia.valor}>
                      {dia.texto}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoHoraInicio">
                  Inicio
                </label>
                <select
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoHoraInicio"
                  name="horaInicio"
                  onChange={cambiarDatoTurnoFijo}
                  value={datosTurnoFijo.horaInicio}
                >
                  {opcionesHorasTurnoFijo.map((hora) => (
                    <option key={hora} value={hora}>
                      {hora}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <p className="text-sm font-bold text-[#0b2f63]">
                  Fin
                </p>
                <div className="mt-1 rounded-xl border border-[#d6dce5] bg-[#edf3ff] px-3 py-2 text-sm font-bold text-[#0b2f63]">
                  {obtenerHoraFinTurnoFijo(datosTurnoFijo.horaInicio)}
                </div>
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoFechaDesde">
                  Desde
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="turnoFijoFechaDesde"
                  name="fechaDesde"
                  onChange={cambiarDatoTurnoFijo}
                  required
                  type="date"
                  value={datosTurnoFijo.fechaDesde}
                />
              </div>

            </div>

            <div className="mt-3">
              <label className="text-sm font-bold text-[#0b2f63]" htmlFor="turnoFijoObservacion">
                Observación
              </label>
              <textarea
                className="mt-1 min-h-20 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                id="turnoFijoObservacion"
                maxLength={300}
                name="observacion"
                onChange={cambiarDatoTurnoFijo}
                value={datosTurnoFijo.observacion}
              />
            </div>

            <button
              className="mt-4 rounded-xl bg-[#0b2f63] px-4 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={creandoTurnoFijo}
              type="submit"
            >
              {creandoTurnoFijo ? 'Creando...' : 'Crear turno fijo'}
            </button>
          </form>

          {mensajeTurnosFijos && (
            <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
              {mensajeTurnosFijos}
            </p>
          )}

          {errorTurnosFijos && (
            <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
              {errorTurnosFijos}
            </p>
          )}

          {!cargandoTurnosFijos && turnosFijos.length === 0 && (
            <div className="mt-5 rounded-xl border border-[#d6dce5] bg-[#f6f8fb] p-4 text-sm font-semibold text-[#566273]">
              No hay turnos fijos cargados.
            </div>
          )}

          <div className="mt-5 grid gap-4 lg:grid-cols-2">
            {turnosFijos.map((turnoFijo) => (
              <article
                className="rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
                key={turnoFijo.turnoFijoId}
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="text-xs font-black uppercase text-[#d6a72b]">
                      {turnoFijo.diaSemanaTexto}
                    </p>
                    <h3 className="mt-1 text-xl font-black text-[#0b2f63]">
                      {turnoFijo.nombreCliente}
                    </h3>
                    <p className="mt-1 text-sm font-semibold text-[#566273]">
                      {turnoFijo.telefonoCliente}
                    </p>
                  </div>

                  <span
                    className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-black ${
                      turnoFijo.activo
                        ? 'bg-[#e9f2ff] text-[#0b2f63]'
                        : 'bg-[#eef1f5] text-[#7c8797]'
                    }`}
                  >
                    {turnoFijo.activo ? 'Activo' : 'Desactivado'}
                  </span>
                </div>

                <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="font-bold text-[#0b2f63]">Horario</dt>
                    <dd>
                      {formatearHoraSimple(turnoFijo.horaInicio)} a{' '}
                      {formatearHoraSimple(turnoFijo.horaFin)}
                    </dd>
                  </div>
                  <div>
                    <dt className="font-bold text-[#0b2f63]">Desde</dt>
                    <dd>{formatearFechaSimple(turnoFijo.fechaDesde)}</dd>
                  </div>
                  {turnoFijo.observacion && (
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Observación</dt>
                      <dd>{turnoFijo.observacion}</dd>
                    </div>
                  )}
                </dl>

                {turnoFijo.activo && (
                  <button
                    className="mt-4 rounded-xl border border-[#9a3d2d] bg-white px-3 py-2 text-sm font-black text-[#9a3d2d] transition hover:bg-[#fff5f2] disabled:cursor-not-allowed disabled:opacity-50"
                    disabled={desactivandoTurnoFijoId === turnoFijo.turnoFijoId}
                    onClick={() => desactivarTurnoFijo(turnoFijo.turnoFijoId)}
                    type="button"
                  >
                    {desactivandoTurnoFijoId === turnoFijo.turnoFijoId
                      ? 'Desactivando...'
                      : 'Desactivar turno fijo'}
                  </button>
                )}
              </article>
            ))}
          </div>
        </section>
        )}

        {panelAdminActivo === 'excepciones' && (
        <section className="order-[4] rounded-[28px] border border-[#d6a72b]/35 bg-white p-5 text-[#061934] shadow-2xl shadow-black/20">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <p className="text-sm font-bold uppercase text-[#d6a72b]">
                Panel del dueño
              </p>
              <h2 className="text-2xl font-black text-[#0b2f63]">
                Feriados y excepciones
              </h2>
            </div>

            <button
              className="rounded-xl border border-[#0b2f63] bg-white px-4 py-2 text-sm font-black text-[#0b2f63] transition hover:bg-[#edf3ff] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={cargandoExcepcionesHorario}
              onClick={obtenerExcepcionesHorario}
              type="button"
            >
              {cargandoExcepcionesHorario ? 'Actualizando...' : 'Actualizar'}
            </button>
          </div>

          <form
            className="mt-5 rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
            onSubmit={crearExcepcionHorario}
          >
            <p className="text-sm font-black uppercase text-[#d6a72b]">
              Nueva excepción
            </p>

            <div className="mt-4 grid gap-3 md:grid-cols-2 lg:grid-cols-4">
              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionFechaDesde">
                  Desde
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="excepcionFechaDesde"
                  name="fechaDesde"
                  onChange={cambiarDatoExcepcionHorario}
                  required
                  type="date"
                  value={datosExcepcionHorario.fechaDesde}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionFechaHasta">
                  Hasta
                </label>
                <input
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="excepcionFechaHasta"
                  name="fechaHasta"
                  onChange={cambiarDatoExcepcionHorario}
                  required
                  type="date"
                  value={datosExcepcionHorario.fechaHasta}
                />
              </div>

              <div>
                <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionTipo">
                  Tipo
                </label>
                <select
                  className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                  id="excepcionTipo"
                  name="tipo"
                  onChange={cambiarDatoExcepcionHorario}
                  value={datosExcepcionHorario.tipo}
                >
                  <option value="cerrado">Cerrado</option>
                  <option value="abierto">Abierto especial</option>
                </select>
              </div>

              {datosExcepcionHorario.tipo === 'abierto' && (
                <>
                  <div>
                    <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionHoraApertura">
                      Apertura
                    </label>
                    <select
                      className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                      id="excepcionHoraApertura"
                      name="horaApertura"
                      onChange={cambiarDatoExcepcionHorario}
                      value={datosExcepcionHorario.horaApertura}
                    >
                      {opcionesHorasTurnoFijo.map((hora) => (
                        <option key={hora} value={hora}>
                          {hora}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionHoraCierre">
                      Cierre
                    </label>
                    <select
                      className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                      id="excepcionHoraCierre"
                      name="horaCierre"
                      onChange={cambiarDatoExcepcionHorario}
                      value={datosExcepcionHorario.horaCierre}
                    >
                      {opcionesHorasTurnoFijo.map((hora) => (
                        <option key={hora} value={hora}>
                          {hora}
                        </option>
                      ))}
                    </select>
                  </div>
                </>
              )}
            </div>

            <div className="mt-3">
              <label className="text-sm font-bold text-[#0b2f63]" htmlFor="excepcionMotivo">
                Motivo
              </label>
              <input
                className="mt-1 w-full rounded-xl border border-[#d6dce5] bg-white px-3 py-2 text-sm outline-none focus:border-[#d6a72b]"
                id="excepcionMotivo"
                maxLength={200}
                name="motivo"
                onChange={cambiarDatoExcepcionHorario}
                placeholder="Feriado, vacaciones, evento especial..."
                type="text"
                value={datosExcepcionHorario.motivo}
              />
            </div>

            <button
              className="mt-4 rounded-xl bg-[#0b2f63] px-4 py-2 text-sm font-black text-white transition hover:bg-[#164d95] disabled:cursor-not-allowed disabled:opacity-50"
              disabled={creandoExcepcionHorario}
              type="submit"
            >
              {creandoExcepcionHorario ? 'Creando...' : 'Crear excepción'}
            </button>
          </form>

          {mensajeExcepcionesHorario && (
            <p className="mt-4 rounded-xl border border-[#b8dfc2] bg-[#f0fff4] p-3 text-sm font-semibold text-[#1e6b35]">
              {mensajeExcepcionesHorario}
            </p>
          )}

          {errorExcepcionesHorario && (
            <p className="mt-4 rounded-xl border border-[#e3b4aa] bg-[#fff5f2] p-3 text-sm font-semibold text-[#9a3d2d]">
              {errorExcepcionesHorario}
            </p>
          )}

          {!cargandoExcepcionesHorario && excepcionesHorario.length === 0 && (
            <div className="mt-5 rounded-xl border border-[#d6dce5] bg-[#f6f8fb] p-4 text-sm font-semibold text-[#566273]">
              No hay excepciones cargadas.
            </div>
          )}

          <div className="mt-5 grid gap-4 lg:grid-cols-2">
            {excepcionesHorario.map((excepcion) => (
              <article
                className="rounded-2xl border border-[#d6dce5] bg-[#f8fafc] p-4"
                key={excepcion.excepcionHorarioId}
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <p className="text-xs font-black uppercase text-[#d6a72b]">
                      {excepcion.fechaDesde === excepcion.fechaHasta
                        ? formatearFechaSimple(excepcion.fechaDesde)
                        : `${formatearFechaSimple(excepcion.fechaDesde)} a ${formatearFechaSimple(excepcion.fechaHasta)}`}
                    </p>
                    <h3 className="mt-1 text-xl font-black text-[#0b2f63]">
                      {excepcion.textoEstado}
                    </h3>
                  </div>

                  <span
                    className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-black ${
                      excepcion.abierto
                        ? 'bg-[#e9f2ff] text-[#0b2f63]'
                        : 'bg-[#fff1df] text-[#9a5b00]'
                    }`}
                  >
                    {excepcion.abierto ? 'Abierto' : 'Cerrado'}
                  </span>
                </div>

                <dl className="mt-4 grid gap-3 text-sm sm:grid-cols-2">
                  <div>
                    <dt className="font-bold text-[#0b2f63]">Horario</dt>
                    <dd>
                      {excepcion.abierto
                        ? `${formatearHoraSimple(excepcion.horaApertura)} a ${formatearHoraSimple(excepcion.horaCierre)}`
                        : 'Sin atención'}
                    </dd>
                  </div>
                  {excepcion.motivo && (
                    <div>
                      <dt className="font-bold text-[#0b2f63]">Motivo</dt>
                      <dd>{excepcion.motivo}</dd>
                    </div>
                  )}
                </dl>

                <button
                  className="mt-4 rounded-xl border border-[#9a3d2d] bg-white px-3 py-2 text-sm font-black text-[#9a3d2d] transition hover:bg-[#fff5f2] disabled:cursor-not-allowed disabled:opacity-50"
                  disabled={eliminandoExcepcionHorarioId === excepcion.excepcionHorarioId}
                  onClick={() => eliminarExcepcionHorario(excepcion.excepcionHorarioId)}
                  type="button"
                >
                  {eliminandoExcepcionHorarioId === excepcion.excepcionHorarioId
                    ? 'Eliminando...'
                    : 'Eliminar excepción'}
                </button>
              </article>
            ))}
          </div>
        </section>
        )}
          </>
        )}
        <footer className="mt-8 border-t border-white/10 py-6 text-left">
          <p className="text-[0.56rem] font-black uppercase tracking-[0.18em] text-white/60 sm:text-[0.68rem] sm:tracking-[0.28em]">
            © 2026 Wilson Futbol 5. Todos los derechos reservados.
          </p>
          <p className="mt-3 text-[0.56rem] font-black uppercase tracking-[0.18em] text-white/60 sm:mt-5 sm:text-[0.68rem] sm:tracking-[0.28em]">
            Desarrollado por
          </p>
          <p className="mt-1 text-xs font-black uppercase tracking-[0.18em] text-white sm:mt-2 sm:text-sm sm:tracking-[0.25em]">
            Enzo Dalmasso
          </p>
        </footer>
      </section>
    </main>
  )
}

export default App
