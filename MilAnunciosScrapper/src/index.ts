import express, { Application } from "express";
import { RegisterRoutes } from './routes';
import swaggerUi from 'swagger-ui-express';

const app: Application = express();
const PORT = process.env.PORT || 8000;

// Middleware
app.use(express.json());
app.use(express.static("public"));

// Swagger setup
app.use(
  "/docs",
  swaggerUi.serve,
  swaggerUi.setup(undefined, {
    swaggerOptions: {
      url: "/swagger.json",
    },
  })
);

// Register routes
RegisterRoutes(app);

app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
  console.log(`Swagger docs available at http://localhost:${PORT}/docs`);
});