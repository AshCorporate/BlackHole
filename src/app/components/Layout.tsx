import { Outlet } from 'react-router';
import { StarBackground } from './StarBackground';

export function Layout() {
  return (
    <div
      className="relative min-h-screen w-full overflow-hidden"
      style={{
        background: 'radial-gradient(ellipse at 20% 50%, #0d0520 0%, #030308 40%, #020105 100%)',
        fontFamily: "'Rajdhani', sans-serif",
      }}
    >
      <StarBackground />
      {/* Nebula accent */}
      <div
        className="fixed pointer-events-none"
        style={{
          top: '-20%',
          right: '-10%',
          width: '60%',
          height: '60%',
          borderRadius: '50%',
          background: 'radial-gradient(circle, #2d0a5e08 0%, transparent 70%)',
          zIndex: 0,
        }}
      />
      <div
        className="fixed pointer-events-none"
        style={{
          bottom: '-20%',
          left: '-10%',
          width: '50%',
          height: '50%',
          borderRadius: '50%',
          background: 'radial-gradient(circle, #0a1a4a08 0%, transparent 70%)',
          zIndex: 0,
        }}
      />
      <div className="relative" style={{ zIndex: 1 }}>
        <Outlet />
      </div>
    </div>
  );
}
