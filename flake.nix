{
  description = "Development environment for Alpha-Kappa";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };
  outputs =
    {
      nixpkgs,
      ...
    }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages."${system}";
    in
    {
      devShell."${system}" = pkgs.mkShell {
        packages = with pkgs; [
          dotnetCorePackages.dotnet_9.sdk
          godot-mono
          jetbrains.rider
        ];
      };
    };
}
