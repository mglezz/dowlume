# Política de ramas

[English](../BRANCHES.md) · [Índice en español](README.md)

Utilizo `main` como única rama permanente. Contiene la aplicación y el README principal en inglés, con acceso a la documentación en español.

## Mi flujo de trabajo

1. Sincronizo `main` sin reescribir el historial.
2. Creo una rama temporal `feature/`, `fix/`, `docs/` o `chore/`.
3. Hago commits acotados con Conventional Commits en inglés y ejecuto las comprobaciones pertinentes.
4. Publico la rama y abro un PR hacia `main`.
5. Reviso el diff, resuelvo los fallos de CI e integro.
6. Elimino la rama integrada y sincronizo `main` local.

```powershell
git switch main
git pull --ff-only origin main
git switch -c feature/describe-the-change
```

No hago force-push del historial compartido. Puedo usar squash para un cambio independiente y merge commits para conservar una serie de ramas dependientes. Documento las comprobaciones en [Contribuciones](CONTRIBUTING.md).

Integré la implementación inicial mediante los [PR #1–#5](https://github.com/mglezz/dowlume/pulls?q=is%3Apr+is%3Amerged+sort%3Acreated-asc) el 15 de octubre de 2026. Conservo sus commits. Publiqué v0.1.0 el 18 de octubre; su identidad está en el [registro de aceptación](RELEASE_CANDIDATE_0.1.0.md).
