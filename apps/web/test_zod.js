const { z } = require('zod');

const schema = z.object({
  cidadeId: z.coerce.number().optional()
});

console.log(schema.parse({ cidadeId: "" }));
console.log(schema.parse({ }));
