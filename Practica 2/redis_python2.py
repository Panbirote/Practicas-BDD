import redis

redis_db = redis.StrictRedis(host='localhost', port=6379, db=0, decode_responses=True)

def menu_alumnos():
    while True:
        print("\n----- Menú Alumnos -----")
        print("1. Agregar alumno")
        print("2. Listar alumnos")
        print("3. Eliminar alumno por ID")
        print("4. Editar datos de un alumno por ID")
        print("5. Regresar al menú principal")
        opcion = input("Seleccione una opción para alumnos: ")

        if opcion == '1':
            agregar_alumno()
        elif opcion == '2':
            print("Listado de alumnos:")
            alumnos = listar_alumnos()
            for alumno in alumnos:
                print(alumno)
        elif opcion == '3':
            eliminar_alumno()
        elif opcion == '4':
            editar_alumno()
        elif opcion == '5':
            break
        else:
            print("Opción no válida. Intente de nuevo.")

def menu_profesores():
    while True:
        print("\n----- Menú Profesores -----")
        print("1. Agregar profesor")
        print("2. Listar profesores")
        print("3. Eliminar profesor por ID")
        print("4. Editar datos de un profesor por ID")
        print("5. Regresar al menú principal")
        opcion = input("Seleccione una opción para profesores: ")

        if opcion == '1':
            agregar_profesor()
        elif opcion == '2':
            print("Listado de profesores:")
            profesores = listar_profesores()
            for profesor in profesores:
                print(profesor)
        elif opcion == '3':
            eliminar_profesor()
        elif opcion == '4':
            editar_profesor()
        elif opcion == '5':
            break
        else:
            print("Opción no válida. Intente de nuevo.")

#######################################################

# Función para agregar un alumno
def agregar_profesor():
    nombre = input("Ingrese el nombre del profesor: ")
    materia = input("Ingrese la materia del profesor: ")
    telefono = input("Ingrese el teléfono del profesor: ")

    profesor_id = redis_db.incr('contador_profesores')
    redis_db.hset(f'profesor:{profesor_id}', 'id', profesor_id)
    redis_db.hset(f'profesor:{profesor_id}', 'nombre', nombre)
    redis_db.hset(f'profesor:{profesor_id}', 'materia', materia)
    redis_db.hset(f'profesor:{profesor_id}', 'telefono', telefono)
    print(f'Profesor agregado con ID {profesor_id}')

# Función para listar todos los profesores
def listar_profesores():
    keys = redis_db.keys('profesor:*')
    profesores = []
    for key in keys:
        profesor = redis_db.hgetall(key)
        profesores.append(profesor)
    return profesores

# Función para obtener un profesor por su ID
def obtener_profesor_por_id(profesor_id):
    profesor = redis_db.hgetall(f'profesor:{profesor_id}')
    if profesor:
        return profesor
    else:
        return None

# Función para eliminar un profesor por su ID
def eliminar_profesor():
    profesor_id = int(input("Ingrese el ID del profesor a eliminar: "))
    profesor = obtener_profesor_por_id(profesor_id)
    if profesor:
        redis_db.delete(f'profesor:{profesor_id}')
        print(f'Profesor con ID {profesor_id} eliminado')
    else:
        print(f'No se encontró ningún profesor con el ID {profesor_id}')

# Función para editar datos de un profesor
def editar_profesor():
    profesor_id = int(input("Ingrese el ID del profesor a editar: "))
    profesor = obtener_profesor_por_id(profesor_id)
    if profesor:
        print(f'Profesor actual: {profesor}')
        nombre = input("Ingrese el nuevo nombre del profesor (deje vacío para mantener sin cambios): ")
        materia = input("Ingrese la nueva materia del profesor (deje vacío para mantener sin cambios): ")
        telefono = input("Ingrese el nuevo teléfono del profesor (deje vacío para mantener sin cambios): ")

        if nombre:
            redis_db.hset(f'profesor:{profesor_id}', 'nombre', nombre)
        if materia:
            redis_db.hset(f'profesor:{profesor_id}', 'materia', materia)
        if telefono:
            redis_db.hset(f'profesor:{profesor_id}', 'telefono', telefono)

        print(f'Datos del profesor con ID {profesor_id} actualizados')
    else:
        print(f'No se encontró ningún profesor con el ID {profesor_id}')

######################################################

# Función para agregar un alumno
def agregar_alumno():
    nombre = input("Ingrese el nombre del alumno: ")
    edad = int(input("Ingrese la edad del alumno: "))
    curso = input("Ingrese el curso del alumno: ")
    grado = input("Ingrese el grado del alumno: ")
    grupo = input("Ingrese el grupo del alumno: ")
    telefono = input("Ingrese el teléfono del alumno: ")

    alumno_id = redis_db.incr('contador_alumnos')
    redis_db.hset(f'alumno:{alumno_id}', 'id', alumno_id)
    redis_db.hset(f'alumno:{alumno_id}', 'nombre', nombre)
    redis_db.hset(f'alumno:{alumno_id}', 'edad', edad)
    redis_db.hset(f'alumno:{alumno_id}', 'curso', curso)
    redis_db.hset(f'alumno:{alumno_id}', 'grado', grado)
    redis_db.hset(f'alumno:{alumno_id}', 'grupo', grupo)
    redis_db.hset(f'alumno:{alumno_id}', 'telefono', telefono)
    print(f'Alumno agregado con ID {alumno_id}')

# Función para listar todos los alumnos
def listar_alumnos():
    keys = redis_db.keys('alumno:*')
    alumnos = []
    for key in keys:
        alumno = redis_db.hgetall(key)
        alumnos.append(alumno)
    return alumnos

# Función para obtener un alumno por su ID
def obtener_alumno_por_id(alumno_id):
    alumno = redis_db.hgetall(f'alumno:{alumno_id}')
    if alumno:
        return alumno
    else:
        return None

# Función para eliminar un alumno por su ID
def eliminar_alumno():
    alumno_id = int(input("Ingrese el ID del alumno a eliminar: "))
    alumno = obtener_alumno_por_id(alumno_id)
    if alumno:
        redis_db.delete(f'alumno:{alumno_id}')
        print(f'Alumno con ID {alumno_id} eliminado')
    else:
        print(f'No se encontró ningún alumno con el ID {alumno_id}')


# Función para editar datos de un alumno
def editar_alumno():
    alumno_id = int(input("Ingrese el ID del alumno a editar: "))
    alumno = obtener_alumno_por_id(alumno_id)
    if alumno:
        print(f'Alumno actual: {alumno}')
        nombre = input("Ingrese el nuevo nombre del alumno (deje vacío para mantener sin cambios): ")
        edad = input("Ingrese la nueva edad del alumno (deje vacío para mantener sin cambios): ")
        curso = input("Ingrese el nuevo curso del alumno (deje vacío para mantener sin cambios): ")
        grado = input("Ingrese el nuevo grado del alumno (deje vacío para mantener sin cambios): ")
        grupo = input("Ingrese el nuevo grupo del alumno (deje vacío para mantener sin cambios): ")
        telefono = input("Ingrese el nuevo teléfono del alumno (deje vacío para mantener sin cambios): ")

        if nombre:
            redis_db.hset(f'alumno:{alumno_id}', 'nombre', nombre)
        if edad:
            redis_db.hset(f'alumno:{alumno_id}', 'edad', edad)
        if curso:
            redis_db.hset(f'alumno:{alumno_id}', 'curso', curso)
        if grado:
            redis_db.hset(f'alumno:{alumno_id}', 'grado', grado)
        if grupo:
            redis_db.hset(f'alumno:{alumno_id}', 'grupo', grupo)
        if telefono:
            redis_db.hset(f'alumno:{alumno_id}', 'telefono', telefono)

        print(f'Datos del alumno con ID {alumno_id} actualizados')
    else:
        print(f'No se encontró ningún alumno con el ID {alumno_id}')

# Menú principal
while True:
    print("\n----- Menú Principal -----")
    print("1. Menu Alumnos")
    print("2. Menu Profesores")
    print("3. Salir")
    opcion_principal = input("Seleccione una opción: ")

    if opcion_principal == '1':
        menu_alumnos()
    elif opcion_principal == '2':
        menu_profesores()
    elif opcion_principal == '3':
        break
    else:
        print("Opción no válida. Intente de nuevo.")