import { Navigate, Route, Routes } from 'react-router'
import RutaProtegida from './components/RutaProtegida'
import Inicio from './pages/Inicio'
import Login from './pages/Login'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route element={<RutaProtegida />}>
        <Route path="/" element={<Inicio />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
