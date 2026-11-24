import { Toaster as Sonner } from "sonner";
import { CircleCheckIcon, InfoIcon, TriangleAlertIcon, OctagonXIcon, Loader2Icon } from "lucide-react"

const Toaster = ({
  ...props
}) => {
  // Siempre claro: el modo oscuro está excluido del Avance 2 (sección 2.2), y "system" seguiría al sistema operativo

  return (
    <Sonner
      theme="light"
      className="toaster group"
      icons={{
        success: (
          <CircleCheckIcon className="size-4" />
        ),
        info: (
          <InfoIcon className="size-4" />
        ),
        warning: (
          <TriangleAlertIcon className="size-4" />
        ),
        error: (
          <OctagonXIcon className="size-4" />
        ),
        loading: (
          <Loader2Icon className="size-4 animate-spin" />
        ),
      }}
      style={
        {
          "--normal-bg": "var(--popover)",
          "--normal-text": "var(--popover-foreground)",
          "--normal-border": "var(--border)",
          "--border-radius": "calc(var(--radius) * 2)"
        }
      }
      toastOptions={{
        // El aviso lleva el tinte de su significado, como los badges: verde éxito, rojo error
        classNames: {
          toast: "cn-toast font-sans text-[0.9375rem]",
          success: "!bg-exito-tinte !text-exito-texto !border-exito/30",
          error: "!bg-alto-tinte !text-alto-texto !border-alto/30",
        },
      }}
      {...props}
    />
  );
}

export { Toaster }
