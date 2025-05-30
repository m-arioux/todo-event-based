import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  async rewrites() {
    return [
      {
        source: "/api/service/:path*",
        destination: `http://todo-service:5000/:path*`,
      },
      {
        source: "/api/persistent/:path*",
        destination: `http://todo-persistent:3000/:path*`,
      },
    ];
  },
  experimental: { clientInstrumentationHook: true },
};

export default nextConfig;
