module.exports = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    // scopes métier TableHall (ajuste mais reste strict)
    'scope-enum': [2, 'always', ['api', 'web', 'dsl', 'db', 'auth', 'offline', 'import', 'export', 'pdf', 'release', 'ci', 'build']],
  },
};
