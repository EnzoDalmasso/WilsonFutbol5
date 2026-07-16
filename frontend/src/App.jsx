import { useEffect, useState } from 'react'
import logoWilson from './assets/LogoWilson.png'

const API_URL = 'https://localhost:7094/api'

const datosReservaIniciales = {
  apellido: '',
  nombre: '',
  telefonoCliente: '',
}

function obtenerFechaHoy() {
  const fecha = new Date()
  const anio = fecha.getFullYear()
  const mes = String(fecha.getMonth() + 1).padStart(2, '0')
  const dia = String(fecha.getDate()).padStart(2, '0')

  return `${anio}-${mes}-${dia}`
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

function formatearPrecio(valor) {
  return new Intl.NumberFormat('es-AR', {
    currency: 'ARS',
    maximumFractionDigits: 0,
    style: 'currency',
  }).format(valor)
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

  async function obtenerDisponibilidad(fecha) {
    setCargando(true)
    setError('')

    try {
      const respuesta = await fetch(`${API_URL}/turnos/disponibilidad?fecha=${fecha}`)

      if (!respuesta.ok) {
        throw new Error('No se pudo obtener la disponibilidad.')
      }

      const datos = await respuesta.json()
      setHorarios(datos)
    } catch (errorActual) {
      setHorarios([])
      setError(errorActual.message)
    } finally {
      setCargando(false)
    }
  }

  // useEffect ejecuta una accion cuando cambia una dependencia.
  // En este caso, cada vez que cambia fechaSeleccionada, pedimos horarios nuevos.
  useEffect(() => {
    obtenerDisponibilidad(fechaSeleccionada)
  }, [fechaSeleccionada])

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

  const precioPorPersona = horarios.length > 0 ? horarios[0].precioPorPersona : 0
  const cantidadDisponibles = horarios.filter((horario) => horario.disponible).length
  const cantidadReservados = horarios.length - cantidadDisponibles

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
                  Reserva tu cancha
                </h1>
              </div>
            </div>

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
          </div>
        </header>

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
                  Telefono
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
                  <p className="text-lg font-black text-[#0b2f63]">Turno confirmado</p>
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
                      <dt className="font-bold text-[#0b2f63]">Telefono</dt>
                      <dd>{turnoConfirmado.telefonoCliente}</dd>
                    </div>
                  </dl>
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

            {!cargando && !error && horarios.length === 0 && (
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
                      {horario.disponible ? 'Disponible' : 'Reservado'}
                    </span>
                  </button>
                )
              })}
            </div>
          </section>
        </section>
      </section>
    </main>
  )
}

export default App
