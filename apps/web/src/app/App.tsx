import { APP_CONFIG } from '../shared/config/app-config';
import './app.css';

export default function App() {
  return (
    <main className="app-container">
      <header>
        <h1>{APP_CONFIG.appName}</h1>
      </header>
      <section className="app-status">
        <p>A fundação técnica do frontend está ativa.</p>
      </section>
      <footer>
        <p>Versão: {APP_CONFIG.version}</p>
        <p>Ambiente: {APP_CONFIG.environment}</p>
      </footer>
    </main>
  );
}
