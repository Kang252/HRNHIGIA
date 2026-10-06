import {
  IsBoolean,
  IsOptional,
  IsString,
  MaxLength,
  MinLength,
} from "class-validator";

export class LoginDto {
  @IsString()
  @MinLength(1)
  @MaxLength(100)
  public username!: string;

  @IsString()
  @MinLength(1)
  @MaxLength(512)
  public password!: string;

  @IsOptional()
  @IsBoolean()
  public rememberMe?: boolean;
}
